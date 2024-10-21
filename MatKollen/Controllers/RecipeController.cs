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

        public RecipeController(RecipeRepository recipeRepository, FoodRepository foodRepository, GroceryListRepository groceryListRepository, GetListsRepository getListsRepository)
        {
            _recipeRepository = recipeRepository;
            _foodRepository = foodRepository;
            _groceryListRepository = groceryListRepository;
            _getListsRepository = getListsRepository;
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
            var existingItems = new List<string>();

            foreach(var item in recipe.Ingredients)
            {
                // Find matching food items between a recipe and the user's food items
                var matchingItem = userFoodItems?.Find(food => food?.UserFoodItems?[0].FoodDetails.FoodItemId == item.IngredientDetails.FoodItemId);

                // Find matching food items between a recipe and the items in the grocery list
                var matchingListItem = _groceryListRepository.GetGroceryList(userId, out error).Find(listItem => listItem.FoodDetails.FoodItemId == item.IngredientDetails.FoodItemId);

                // If the item exists among the user's food items and the amount is equal to or more than in the recipe
                if (matchingItem != null && (matchingItem?.UserFoodItems?[0].FoodDetails.Quantity >= item.IngredientDetails.Quantity))
                {
                    item.UserHasIngredient = true;   
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
                }

                if (_groceryListRepository.GroceryListItemsExists(item.IngredientDetails.FoodItemId, userId, out error) && (matchingListItem.FoodDetails.Quantity >= item.IngredientDetails.Quantity))
                {
                    existingItems.Add(item.Ingredient);
                }
            }

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
            List<RecipeCategory> categoryList = _getListsRepository.GetRecipeCategories(out string error);

            if (error != "")
            {
                TempData["error"] = error;
            }

            ViewData["categories"] = categoryList;

            return View();
        }

        [HttpPost]       
        public IActionResult Create(Recipe recipe)
        {
            if (!ModelState.IsValid)
            {
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
        public IActionResult AddIngredient(RecipeFoodItem ingredient)
        {
            if (!ModelState.IsValid)
            {
                return View(ingredient);
            }
            
            var quantityConverter = new ConvertQuantityHandler();
            var measurementMultipliers = _getListsRepository.GetUnits(out string unitsError);
            if (unitsError != "")
            {
                TempData["error"] = unitsError;
                return View(ingredient);
            }
            double multiplier = measurementMultipliers.Find(m => m.Id == ingredient.UnitId).Multiplier;
            ingredient.Quantity = quantityConverter.ConverToLiterOrKg(ingredient.Quantity, multiplier);

            var affectedRows = _recipeRepository.InsertIngredient(ingredient, out string error);
            if (error != "" || affectedRows == 0) TempData["error"] = "Det gick inte att lägga till ingrediensen."  + error;
            
            return RedirectToAction("details", new {id = ingredient.RecipeId});
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
            return View();
        }

        [HttpPost]
        //Recipe/Delete
        public IActionResult Delete(int id)
        {
            return View();
        }


    }
}