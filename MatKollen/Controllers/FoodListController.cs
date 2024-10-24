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
        private readonly ConvertQuantityHandler _convertQuantityHandler;

        public FoodListController(FoodRepository foodRepository, UnitsRepository unitsRepository, FoodCategoriesRepository foodCategoriesRepository, ConvertQuantityHandler convertQuantityHandler)
        {
            _foodRepository = foodRepository;
            _unitRepository = unitsRepository;
            _foodCategoryRepository = foodCategoriesRepository;
            _convertQuantityHandler = convertQuantityHandler;
        }
        
        [HttpGet]
        public IActionResult AddNewFoodItem()
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
        public IActionResult AddNewFoodItem(FoodAndUserFoodItemViewModel item)
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
            int affectedRows = _foodRepository.InsertFoodItem(item, out string error);

            if (error != "")
            {
                TempData["error"] = "Det gick inte att lägga till matvaran:" + error;
            }

            if (affectedRows == 0) TempData["error"] = "Det gick inte att lägga till matvaran";
            else TempData["success"] = "Matvara tillagd!";

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult AddNewIngredientFoodItem(int id)
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
        public IActionResult AddNewIngredientFoodItem(IngredientAndFoodItemViewModel item)
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
            int affectedRows = _foodRepository.InsertIngredientFoodItem(item, out string error);

            if (error != "")
            {
                TempData["error"] = "Det gick inte att lägga till matvaran:" + error;
            }

            if (affectedRows == 0) TempData["error"] = "Det gick inte att lägga till matvaran";
            else TempData["success"] = "Matvara tillagd!";

            return RedirectToAction("Edit", "Recipe", new {id = item.RecipeFoodItem.RecipeId});
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