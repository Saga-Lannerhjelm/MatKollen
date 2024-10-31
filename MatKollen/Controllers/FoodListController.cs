using MatKollen.Controllers.Repositories;
using MatKollen.DAL.Repositories;
using MatKollen.Models;
using MatKollen.Services;
using MatKollen.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace MatKollen.Controllers
{
    public class FoodListController : Controller
    {
        private readonly FoodRepository _foodRepository;
        private readonly UnitsRepository _unitRepository;
        private readonly FoodCategoriesRepository _foodCategoryRepository;
        private readonly GroceryListRepository _groceryListRepository;
        private readonly ConvertQuantityHandler _convertQuantityHandler;

        public FoodListController(FoodRepository foodRepository, UnitsRepository unitsRepository, FoodCategoriesRepository foodCategoriesRepository, ConvertQuantityHandler convertQuantityHandler, GroceryListRepository groceryListRepository)
        {
            _foodRepository = foodRepository;
            _unitRepository = unitsRepository;
            _foodCategoryRepository = foodCategoriesRepository;
            _groceryListRepository = groceryListRepository;
            _convertQuantityHandler = convertQuantityHandler;
        }
        
        [HttpGet]
        public IActionResult AddNewFoodToUserList()
        {
            var model = new FoodAndUserFoodItemViewModel
            {
                FoodItem = new FoodItem(),
                UserFoodItem = new UserFoodItem()
            };
            GetUnitsAndCategories();
            return View(model);
        }

        [HttpPost]
        public IActionResult AddNewFoodToUserList(FoodAndUserFoodItemViewModel item)
        {
            if (item.UserFoodItem.ExpirationDate < DateOnly.FromDateTime(DateTime.Now))
            {
                ModelState.AddModelError("expirationDate", "Utgångsdatumet kan inte vara i det förflutna");
            }

            if (!ModelState.IsValid)
            {
                GetUnitsAndCategories();
                return View(item);
            }

            // Find multiplier based on unit id
            var unitList = _unitRepository.GetUnits(out string unitsError);
            if (unitsError != "")
            {
                TempData["error"] = unitsError;
                return View(item);
            }
            double multiplier = unitList.Find(m => m.Id == item.UserFoodItem.UnitId).Multiplier;

            //Get user id
            int userId = UserHelper.GetUserId(User);
            item.UserFoodItem.UserId = userId;

            // Recalculate quantity
            item.UserFoodItem.Quantity = _convertQuantityHandler.ConverToLiterOrKg(item.UserFoodItem.Quantity, multiplier);

            // Insert values in database
            int affectedRows = _foodRepository.InsertFoodAndAssignToUserInventory(item, out string error);

            if (error != "")
            {
                TempData["error"] = "Det gick inte att lägga till matvaran:" + error;
            }

            if (affectedRows == 0) TempData["error"] = "Det gick inte att lägga till matvaran";
            else TempData["success"] = "Matvara tillagd!";

            return RedirectToAction("Index", "UserFoodItems");
        }

        [HttpGet]
        public IActionResult AddNewIngredientToRecipe(int id)
        {
            var model = new IngredientAndFoodItemViewModel
            {
                FoodItem = new FoodItem(),
                RecipeFoodItem = new RecipeFoodItem()
                {
                    RecipeId = id,
                }
            };
            GetUnitsAndCategories();
            return View(model);
        }

        [HttpPost]
        public IActionResult AddNewIngredientToRecipe(IngredientAndFoodItemViewModel item)
        {
            if (!ModelState.IsValid)
            {
                GetUnitsAndCategories();
                return View(item);
            }

            // Find multiplier based on unit id
            var unitList = _unitRepository.GetUnits(out string unitsError);
            if (unitsError != "")
            {
                TempData["error"] = unitsError;
                return View(item);
            }
            double multiplier = unitList.Find(m => m.Id == item.RecipeFoodItem.UnitId).Multiplier;


            // Recalculate quantity
            item.RecipeFoodItem.Quantity = _convertQuantityHandler.ConverToLiterOrKg(item.RecipeFoodItem.Quantity, multiplier);

            // Insert values in database
            int affectedRows = _foodRepository.InsertFoodAndAssignToRecipe(item, out string error);

            if (error != "")
            {
                TempData["error"] = "Det gick inte att lägga till matvaran:" + error;
            }

            if (affectedRows == 0) TempData["error"] = "Det gick inte att lägga till matvaran";
            else TempData["success"] = "Matvara tillagd!";

            return RedirectToAction("Edit", "Recipe", new {id = item.RecipeFoodItem.RecipeId});
        }

        [HttpGet]
        public IActionResult AddNewFoodToGroceryList()
        {
            int userId = UserHelper.GetUserId(User);
            int listId = _groceryListRepository.GetGroceryListId(userId, out string error);

            if (listId != 0)
            {
                var model = new FoodAndGroceryListFoodViewModel
                {
                    FoodItem = new FoodItem(),
                    GroceryListFoodItem = new GroceryListFoodItem()
                    {
                        ListId = listId,
                    }
                };
                GetUnitsAndCategories();
                return View(model);
            } else
            {
                TempData["error"] = "Ingen lista hittades";
            }

            if (error != "")
            {
                TempData["error"] = error;
            }
            return RedirectToAction("Index", "GroceryList");
        }

        [HttpPost]
        public IActionResult AddNewFoodToGroceryList(FoodAndGroceryListFoodViewModel item)
        {
            if (!ModelState.IsValid)
            {
                GetUnitsAndCategories();
                return View(item);
            }

            // Find multiplier based on unit id
            var unitList = _unitRepository.GetUnits(out string unitsError);
            if (unitsError != "")
            {
                TempData["error"] = unitsError;
                return View(item);
            }
            double multiplier = unitList.Find(m => m.Id == item.GroceryListFoodItem.UnitId).Multiplier;


            // Recalculate quantity
            item.GroceryListFoodItem.Quantity = _convertQuantityHandler.ConverToLiterOrKg(item.GroceryListFoodItem.Quantity, multiplier);

            // Insert values in database
            int affectedRows = _foodRepository.InsertFoodAndAssignToGroceryList(item, out string error);

            if (error != "")
            {
                TempData["error"] = "Det gick inte att lägga till matvaran:" + error;
            }

            if (affectedRows == 0) TempData["error"] = "Det gick inte att lägga till matvaran";
            else TempData["success"] = "Matvara tillagd!";

            return RedirectToAction("Index", "GroceryList");
        }

        private void GetUnitsAndCategories()
        {
            var unitList = _unitRepository.GetUnits(out string unitsError);
            var categoryList = _foodCategoryRepository.GetFoodCategories(out string categoryError);

            if (unitsError != "" || categoryError != "") TempData["error"] = unitsError + " " + categoryError;

            ViewData["units"] = unitList;
            ViewData["categories"] = categoryList;
        }

        

    }
}