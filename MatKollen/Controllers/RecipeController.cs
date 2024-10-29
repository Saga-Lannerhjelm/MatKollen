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
        private readonly UserFoodItemRepository _userFoodItemRepository;
        private readonly GroceryListRepository _groceryListRepository;
        private readonly RecipeCategoriesRepository _recipeCategoriesRepository;
        private readonly UnitsRepository _unitRepository;
        private readonly ConvertQuantityHandler _convertQuantityHandler;

        public RecipeController(
            RecipeRepository recipeRepository, 
            FoodRepository foodRepository,
            UserFoodItemRepository userFoodItemRepository,
            GroceryListRepository groceryListRepository, 
            RecipeCategoriesRepository recipeCategoriesRepository, 
            UnitsRepository unitsRepository,
            ConvertQuantityHandler convertQuantityHandler
        )
        {
            _recipeRepository = recipeRepository;
            _foodRepository = foodRepository;
            _userFoodItemRepository = userFoodItemRepository;
            _groceryListRepository = groceryListRepository;
            _recipeCategoriesRepository = recipeCategoriesRepository;
            _unitRepository = unitsRepository;
            _convertQuantityHandler = convertQuantityHandler;
        }

        public IActionResult Index(string searchPrompt, int categoryId)
        {   
            var recipeList = _recipeRepository.GetRecipes(searchPrompt, categoryId, out string error);

            if (error != "")
            {
                 TempData["error"] = error;
                 return RedirectToAction("Index", "UserFoodItems");
            }

            var recipeCategories = _recipeCategoriesRepository.GetRecipeCategories(out string categoryError);

            if (categoryError != "")
            {
                TempData[error] = categoryError;
                return RedirectToAction("Index", "UserFoodItems");
            } else
            {
                ViewData["categories"] = recipeCategories;
            }

            ViewBag.searchPrompt = searchPrompt;
            ViewBag.category = categoryId;
            return View(recipeList);
        }

        public IActionResult Details(int id)
        {
            // Fetch the fooditems in the user's inventory
            int userId = UserHelper.GetUserId(User);
            // Get the recipe and ingredients based on id
            var recipe = _recipeRepository.GetRecipe(id, userId, out string error);

            var missingIngredients = new List<GroceriesToAddViewModel>();
            var existingIngredientsInGroceryList = new Dictionary<int, string>();

            // loop through each ingredient in the recipe
            foreach(var ingredient in recipe.Ingredients)
            {
                if (ingredient.UserHasIngredient == false)
                {
                    var foodItem = new GroceriesToAddViewModel()
                    {
                        ListItem = new GroceryListFoodItem()
                        {
                            Quantity = ingredient.IngredientDetails.Quantity,
                            UnitId = ingredient.IngredientDetails.UnitId,
                            FoodItemId = ingredient.IngredientDetails.FoodItemId,
                        },
                        UnitType = ingredient.UnitInfo.Type
                    };

                    // The non-matching food items are added to a list
                    missingIngredients.Add(foodItem);

                    // Find matching food items between a recipe and the items in the grocery list
                    var matchingGroceryListItem = _groceryListRepository.GetGroceryList(userId, out error).Find(listItem => listItem.FoodDetails.FoodItemId == ingredient.IngredientDetails.FoodItemId);

                    // If the food item exist in the grocery list and the ammount is bigger or equal to the ingredient in the recipe
                    if (_groceryListRepository.GroceryListItemsExists(ingredient.IngredientDetails.FoodItemId, ingredient.UnitInfo.Type, userId, out error) && (matchingGroceryListItem?.FoodDetails.Quantity >= ingredient.IngredientDetails.Quantity))
                    {
                        existingIngredientsInGroceryList.Add(ingredient.IngredientDetails.FoodItemId, ingredient.Ingredient);
                    }
                }
            }

            // Saves the non-matching food items to a session variabel
            HttpContext.Session.SetObject("groceryList", missingIngredients);

            ViewBag.user = userId;
            ViewBag.groceryListItemsExists = existingIngredientsInGroceryList;
            ViewBag.itemsAreMissing = missingIngredients.Count > 0;
            return View(recipe);
        }

         public IActionResult MyRecipes()
        {
            int userId = UserHelper.GetUserId(User);

            var recipeList = _recipeRepository.GetUsersRecipes(userId, out string error);
            if (error != "")
            {
                TempData["error"] = error;
            }

            return View(recipeList);
        }

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
            var userId = UserHelper.GetUserId(User);
            recipe.UserId = userId;

            int recipeId = _recipeRepository.InsertRecipe(recipe, out string error);
            if (recipeId == 0 || error != "")
            {
                 TempData["error"] = "Gick inte att skapa receptet. " + error;
                 return View(recipe);
            }
            return RedirectToAction("AddIngredient", new {recipeId, title = recipe.Title});
        }

        private List<RecipeCategory> GetCategories()
        {
            List<RecipeCategory> categoryList = _recipeCategoriesRepository.GetRecipeCategories(out string error);

            if (error != "")
            {
                TempData["error"] = error;
            }

            return categoryList;
        }

        [HttpGet]       
        public IActionResult AddIngredient(int recipeId, string title)
        {
            List<MeasurementUnit> unitList = _unitRepository.GetUnits(out string unitsError);
            List<FoodItem>? foodItemList = _foodRepository.GetFoodItems(out string error);

            if (unitsError != "" || error != "") 
            {
                TempData["error"] = unitsError + " " + error;
                return RedirectToAction("MyRecipes");
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
            List<MeasurementUnit> unitList = _unitRepository.GetUnits(out string unitsError);
            string error = "";
            if (!ModelState.IsValid)
            {
                List<FoodItem>? foodItemList = _foodRepository.GetFoodItems(out error);

                if (unitsError != "" || error != "") {
                    TempData["error"] = unitsError + " " + error;
                    return RedirectToAction("MyRecipes");
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
        public IActionResult Edit(int id)
        {
            string error = "";
            int userId = UserHelper.GetUserId(User);
            var recipe = _recipeRepository.GetRecipe(id, userId, out error);
            if (error != "") TempData["error"] = error;

            var categories = _recipeCategoriesRepository.GetRecipeCategories(out error);
            if (categories == null) TempData["error"] = "Gick inte att hämta kategorier";

            if (error != "") TempData["error"] = error;
            else ViewData["categories"] = categories;
            return View(recipe);
        }

        [HttpPost]
        public IActionResult Edit(Recipe recipe)
        {
            var affectedRows = _recipeRepository.Update(recipe, out string error);
            if (error != "" || affectedRows == 0) 
            {
               TempData["error"] = error;
               return RedirectToAction("MyRecipes");
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
        public IActionResult DeleteIngredient(int id, int recipeId)
        {
            var affectedRows = _recipeRepository.DeleteIngredient(id, out string error);
            if (affectedRows == 0) TempData["error"] = "Det gick inte att ta bort ingrediensen";
            if (error != "") TempData["error"] = error;
            return RedirectToAction("Edit", new {id = recipeId});
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var affectedRows = _recipeRepository.Delete(id, out string error);
            if (affectedRows == 0) TempData["error"] = "Det gick inte att ta bort receptet";
            if (error != "") TempData["error"] = error;
            return RedirectToAction("MyRecipes");
        }
    }
}