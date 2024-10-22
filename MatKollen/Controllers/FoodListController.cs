using MatKollen.Controllers.Repositories;
using MatKollen.DAL.Repositories;
using MatKollen.Models;
using MatKollen.Services;
using MatKollen.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Mysqlx;

namespace MatKollen.Controllers
{
    public class FoodListController : Controller
    {
        private readonly FoodRepository _foodRepository;
        private readonly GetListsRepository _getListRepository;

        public FoodListController(FoodRepository foodRepository, GetListsRepository getListsRepository)
        {
            _foodRepository = foodRepository;
            _getListRepository = getListsRepository;
        }
        
        //FoodList
        public IActionResult Index(string showAccordionName)
        {
            var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "id").Value);
            var foodList = _foodRepository.GetUserFoodList(userId, out string error);
            
            if (error != "") TempData["error"] = error;

            ViewBag.showAccordionName = showAccordionName;

            return View(foodList);
        }

        //FoodList/Add
        [HttpGet]
        public IActionResult Add()
        {
            var model = new UserFoodItem();

            GetAddFormData();
            return View(model);
        }

        [HttpPost]
        public IActionResult Add(UserFoodItem item)
        {
            if (item.ExpirationDate < DateOnly.FromDateTime(DateTime.Now))
            {
                ModelState.AddModelError(nameof(item.ExpirationDate), "Utgångsdatumet kan inte vara i det förflutna");
            }

            if (!ModelState.IsValid)
            {
                GetAddFormData();
                return View(item);
            }

            // Find multiplier based on unit id
            var measurementMultipliers = _getListRepository.GetUnits(out string unitsError);
            if (unitsError != "")
            {
                TempData["error"] = unitsError;
                return View(item);
            }
            double multiplier = measurementMultipliers.Find(m => m.Id == item.UnitId).Multiplier;

            //Get user id
            var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "id").Value);
            item.UserId = userId;

            // Recalculate quantity
            var conversionHandler = new ConvertQuantityHandler();
            item.Quantity = conversionHandler.ConverToLiterOrKg(item.Quantity, multiplier);

            // Insert values in database
            int affectedRows = _foodRepository.AddFoodItem(item, out string error);

            if (error != "")
            {
                TempData["error"] = "Det gick inte att lägga till matvaran:" + error;
            }

            if (affectedRows == 0) TempData["error"] = "Det gick inte att lägga till matvaran";
            else TempData["success"] = "Matvara tillagd!";

            return RedirectToAction("Index");
        }

        private void GetAddFormData()
        {
            var unitList = _getListRepository.GetUnits(out string unitsError);
            List<FoodItem>? foodItemList = _foodRepository.GetFoodItems(out string error);

            if (unitsError != "" || error != "") TempData["error"] = unitsError + " " + error;

            ViewData["units"] = unitList;
            ViewData["foodItems"] = foodItemList;
        }

        //FoodList/AddNewFoodItem
        [HttpGet]
        public IActionResult AddNewFoodItem()
        {
            var model = new AddFoodAndUserItemViewModel
            {
                FoodItem = new FoodItem(),
                UserFoodItem = new UserFoodItem()
            };
            GetFormLists();
            return View(model);
        }

        [HttpPost]
        public IActionResult AddNewFoodItem(AddFoodAndUserItemViewModel item)
        {
            if (item.UserFoodItem.ExpirationDate < DateOnly.FromDateTime(DateTime.Now))
            {
                ModelState.AddModelError("expirationDate", "Utgångsdatumet kan inte vara i det förflutna");
            }

            if (!ModelState.IsValid)
            {
                GetFormLists();
                return View(item);
            }

            // Find multiplier based on unit id
            var measurementMultipliers = _getListRepository.GetUnits(out string unitsError);
            if (unitsError != "")
            {
                TempData["error"] = unitsError;
                return View(item);
            }
            double multiplier = measurementMultipliers.Find(m => m.Id == item.UserFoodItem.UnitId).Multiplier;

            //Get user id
            var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "id").Value);
            item.UserFoodItem.UserId = userId;

            // Recalculate quantity
            var conversionHandler = new ConvertQuantityHandler();
            item.UserFoodItem.Quantity = conversionHandler.ConverToLiterOrKg(item.UserFoodItem.Quantity, multiplier);

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

        //FoodList/AddNewIngredientFoodItem
        [HttpGet]
        public IActionResult AddNewIngredientFoodItem(int id)
        {
            var model = new NewIngredientViewModel
            {
                FoodItem = new FoodItem(),
                RecipeFoodItem = new RecipeFoodItem()
                {
                    RecipeId = id,
                }
            };
            GetFormLists();
            return View(model);
        }

        [HttpPost]
        public IActionResult AddNewIngredientFoodItem(NewIngredientViewModel item)
        {
            if (!ModelState.IsValid)
            {
                GetFormLists();
                return View(item);
            }

            // Find multiplier based on unit id
            var measurementMultipliers = _getListRepository.GetUnits(out string unitsError);
            if (unitsError != "")
            {
                TempData["error"] = unitsError;
                return View(item);
            }
            double multiplier = measurementMultipliers.Find(m => m.Id == item.RecipeFoodItem.UnitId).Multiplier;


            // Recalculate quantity
            var conversionHandler = new ConvertQuantityHandler();
            item.RecipeFoodItem.Quantity = conversionHandler.ConverToLiterOrKg(item.RecipeFoodItem.Quantity, multiplier);

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

        private void GetFormLists()
        {
            var unitList = _getListRepository.GetUnits(out string unitsError);
            var categoryList = _getListRepository.GetFoodCategories(out string categoryError);

            if (unitsError != "" || categoryError != "") TempData["error"] = unitsError + " " + categoryError;

            ViewData["units"] = unitList;
            ViewData["categories"] = categoryList;
        }

        //FoodList/Edit
        [HttpGet]
        public IActionResult Edit(int id)
        {
            return View();
        }

        [HttpPost]
        public IActionResult Edit(FoodItem item)
        {
            return View();
        }

        [HttpPost]
        public IActionResult IncreaseQuantity(int id, double unitMultiplier, double quantity, string unit, string showAccordionName)
        {
            double incrementNr = unit != "kg" && unit != "L" ? 1 / unitMultiplier : 0.1;
            var affectedRows = _foodRepository.UpdateQuantity(id, incrementNr, out string error);
            if (error != "") TempData["error"] = error;
            if (affectedRows == 0) TempData["error"] = "Det gick inte att ändra antalet";
            return RedirectToAction("Index", new {showAccordionName});
        }

        [HttpPost]
        public IActionResult DecreaseQuantity(int id, double unitMultiplier, double quantity, string unit, string showAccordionName)
        {
            if (quantity > 0)
            {
                double decreaseNr = (unit != "kg" && unit != "L" ? 1 / unitMultiplier : 0.1) * -1;
                var affectedRows = _foodRepository.UpdateQuantity(id, decreaseNr, out string error);
                if (error != "") TempData["error"] = error;
                if (affectedRows == 0) TempData["error"] = "Det gick inte att ändra antalet";
            }
            return RedirectToAction("Index", new {showAccordionName});
        }


        [HttpPost]
        public IActionResult Delete(int foodId, int userId, string type)
        {
            var affectedRows = _foodRepository.Delete(foodId, userId, type, out string error);

            if (error != "") TempData["error"] = error;
            if (affectedRows == 0) TempData["error"] = "Det gick inte att ta bort varan";
            return RedirectToAction("index");
        }

    }
}