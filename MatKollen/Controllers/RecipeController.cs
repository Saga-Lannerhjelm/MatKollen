using MatKollen.Controllers.Repositories;
using MatKollen.DAL.Repositories;
using MatKollen.Extensions;
using MatKollen.Models;
using MatKollen.Services;
using MatKollen.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Mysqlx;

namespace MatKollen.Controllers
{
    public class RecipeController : Controller
    {
        private readonly RecipeRepository _recipeRepository;
        private readonly FoodRepository _foodRepository;
        private readonly GroceryListRepository _groceryListRepository;
        private readonly GetListsRepository _getListsRepository;
        private readonly ConvertQuantityHandler _convertQuantityHandler;

        public RecipeController(
            RecipeRepository recipeRepository, 
            FoodRepository foodRepository, 
            GroceryListRepository groceryListRepository, 
            GetListsRepository getListsRepository, 
            ConvertQuantityHandler convertQuantityHandler
        )
        {
            _recipeRepository = recipeRepository;
            _foodRepository = foodRepository;
            _groceryListRepository = groceryListRepository;
            _getListsRepository = getListsRepository;
            _convertQuantityHandler = convertQuantityHandler;
        }

        //Recipe
        public IActionResult Index()
        {   
            var recipeList = _recipeRepository.GetRecipes(out string error);
            return View(recipeList);
        }

        //Recipe/Details
        public IActionResult Details(int id)
        {
            var recipe = _recipeRepository.GetRecipe(id, out string error);

            int userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "id").Value);
            var userFoodItems = _foodRepository.GetUserFoodList(userId, out string listError);
            var foodItemsForGroceryList = new List<ListFoodItem>();
            var existingItems = new Dictionary<int, string>();

            foreach(var item in recipe.Ingredients)
            {
                // Find matching food items between a recipe and the user's food items
                var matchingItem = userFoodItems?.Find(food => food?.UserFoodItems?[0].FoodDetails.FoodItemId == item.IngredientDetails.FoodItemId);

                // Find matching food items between a recipe and the items in the grocery list
                var matchingGroceryListItem = _groceryListRepository.GetGroceryList(userId, out error).Find(listItem => listItem.FoodDetails.FoodItemId == item.IngredientDetails.FoodItemId);

                // If the item exists among the user's food items and the amount is equal to or more than in the recipe
                if (matchingItem != null && (matchingItem?.UserFoodItems?[0].FoodDetails.Quantity >= item.IngredientDetails.Quantity))
                {
                    item.UserHasIngredient = true;   
                }
                else if (matchingItem != null && (matchingItem?.UserFoodItems[0].UnitInfo.Type != item.Type)) {
                    item.UserHasIngredient = true;
                    item.IngredientExistInOtherType = true;
                }
                else
                {
                    var foodItem = new ListFoodItem()
                    {
                        Quantity = item.IngredientDetails.Quantity,
                        UnitId = item.IngredientDetails.UnitId,
                        FoodItemId = item.IngredientDetails.FoodItemId,
                    };

                    // The non-matching food items are added to a list
                    foodItemsForGroceryList.Add(foodItem);
                    if (_groceryListRepository.GroceryListItemsExists(item.IngredientDetails.FoodItemId, userId, out error) && (matchingGroceryListItem.FoodDetails.Quantity >= item.IngredientDetails.Quantity))
                    {
                        existingItems.Add(item.IngredientDetails.FoodItemId, item.Ingredient);
                    }
                }
            }

            ViewBag.user = userId;
            ViewBag.groceryListItemsExists = existingItems;
            // Saves the non-matching food items to a session variabel
            HttpContext.Session.SetObject("groceryList", foodItemsForGroceryList);

            return View(recipe);
        }

        //Recipe/Saved
        public IActionResult My()
        {
            var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value);
            
            var recipeList = _recipeRepository.GetUsersRecipes(userId, out string error);
            return View(recipeList);
        }

        //Recipe/Create
        [HttpGet]       
        public IActionResult Create()
        {
            List<RecipeCategory> categoryList = GetCategories();

            ViewData["categories"] = categoryList;

            return View();
        }

        [HttpPost]       
        public IActionResult Create(Recipe recipe)
        {
            if (!ModelState.IsValid)
            {
                List<RecipeCategory> categoryList = GetCategories();
                 ViewData["categories"] = categoryList;
                return View(recipe);
            }
            var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value);
            recipe.UserId = userId;

            int recipeId = _recipeRepository.InsertRecipe(recipe, out string error);
            if (recipeId == 0 || error != "")
            {
                 TempData["error"] = "Gick inte att skapa receptet. " + error;
                 return View();
            }
            return RedirectToAction("AddIngredient", new {recipeId, title = recipe.Title});
        }

        private List<RecipeCategory> GetCategories()
        {
            List<RecipeCategory> categoryList = _getListsRepository.GetRecipeCategories(out string error);

            if (error != "")
            {
                TempData["error"] = error;
            }

            return categoryList;
        }

        //Recipe/AddIngredient
        [HttpGet]       
        public IActionResult AddIngredient(int recipeId, string title)
        {
            List<MeasurementUnit> unitList = _getListsRepository.GetUnits(out string unitsError);
            List<FoodItem>? foodItemList = _foodRepository.GetFoodItems(out string error);

            if (unitsError != "" || error != "") {
                TempData["error"] = unitsError + " " + error;
                return RedirectToAction("My");
            }

            ViewData["units"] = unitList;
            ViewData["foodItems"] = foodItemList;

            var model = new RecipeFoodItem()
            {
                RecipeId = recipeId
            };
            ViewBag.recipe = title;
            return View(model);
        }

        [HttpPost]       
        public IActionResult AddIngredient(RecipeFoodItem ingredient, string title)
        {
            List<MeasurementUnit> unitList = _getListsRepository.GetUnits(out string unitsError);
            string error = "";
            if (!ModelState.IsValid)
            {
                List<FoodItem>? foodItemList = _foodRepository.GetFoodItems(out error);

                if (unitsError != "" || error != "") {
                    TempData["error"] = unitsError + " " + error;
                    return RedirectToAction("My");
                }

                ViewData["units"] = unitList;
                ViewData["foodItems"] = foodItemList;

                ViewBag.recipe = title;
                return View(ingredient);
            }
            
            if (unitsError != "")
            {
                TempData["error"] = unitsError;
                return View(ingredient);
            }
            double multiplier = unitList.Find(m => m.Id == ingredient.UnitId).Multiplier;
            ingredient.Quantity = _convertQuantityHandler.ConverToLiterOrKg(ingredient.Quantity, multiplier);

            var affectedRows = _recipeRepository.InsertIngredient(ingredient, out error);
            if (error != "" || affectedRows == 0) TempData["error"] = "Det gick inte att lägga till ingrediensen."  + error;
            
            return RedirectToAction("edit", new {id = ingredient.RecipeId});
        }

        [HttpGet]
        //Recipe/Edit
        public IActionResult Edit(int id)
        {
            string error = "";
            var recipe = _recipeRepository.GetRecipe(id, out error);
            var categories = _getListsRepository.GetRecipeCategories(out error);

            if (error != "") TempData["error"] = error;
            if (categories == null) TempData["error"] = "Gick inte att hämta kategorier";
            else ViewData["categories"] = categories;
            return View(recipe);
        }

        [HttpPost]
        //Recipe/Edit
        public IActionResult Edit(Recipe recipe)
        {
            var affectedRows = _recipeRepository.Update(recipe, out string error);
            if (error != "" || affectedRows == 0) 
            {
               TempData["error"] = error;
               return RedirectToAction("My");
            }
            else
            {
                TempData["Success"] = "Receptet har uppdaterats";
            } 

            return RedirectToAction("Edit", new {id = recipe.Id});
        }

        [HttpPost]
        public IActionResult IncreaseQuantity(int id, double unitMultiplier, string unit, int recipeId)
        {
            decimal incrementNr = Convert.ToDecimal(unit != "kg" && unit != "L" ? 1 / unitMultiplier : 0.1);
            var affectedRows = _recipeRepository.UpdateQuantity(id, incrementNr, out string error);
            if (error != "") TempData["error"] = error;
            if (affectedRows == 0) TempData["error"] = "Det gick inte att ändra antalet";
            return RedirectToAction("Edit", new {id = recipeId});
        }

        [HttpPost]
        public IActionResult DecreaseQuantity(int id, double unitMultiplier, decimal quantity, string unit, int recipeId)
        {
            if (quantity > 0)
            {
                decimal decreaseNr = Convert.ToDecimal((unit != "kg" && unit != "L" ? 1 / unitMultiplier : 0.1) * -1);
                var affectedRows = _recipeRepository.UpdateQuantity(id, decreaseNr, out string error);
                if (error != "") TempData["error"] = error;
                if (affectedRows == 0) TempData["error"] = "Det gick inte att ändra antalet";
            }
            return RedirectToAction("Edit", new {id = recipeId});
        }

        [HttpPost]
        //Recipe/Delete
        public IActionResult Delete(int id, int recipeId)
        {
            var affectedRows = _recipeRepository.Delete(id, out string error);
            if (error != "") TempData["error"] = error;
            if (affectedRows == 0) TempData["error"] = "Det gick inte att ta bort receptet";
            return RedirectToAction("Edit", new {id = recipeId});
        }

        [HttpPost]
        //Recipe/Delete
        public IActionResult DeleteIngredient(int id)
        {
            var affectedRows = _recipeRepository.DeleteIngredient(id, out string error);
            if (error != "") TempData["error"] = error;
            if (affectedRows == 0) TempData["error"] = "Det gick inte att ta bort ingrediensen";
            return RedirectToAction("My");
        }


    }
}