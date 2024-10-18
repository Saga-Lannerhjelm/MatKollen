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
        public IActionResult Index()
        {
            var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "id").Value);
            var foodList = _foodRepository.GetUserFoodList(userId, out string error);
            
            if (error != "") TempData["error"] = error;

            return View(foodList);
        }

        //FoodList/Add
        [HttpGet]
        public IActionResult Add()
        {
            List<FoodItem>? foodItemList = _foodRepository.GetFoodItems(out string error);

            if (error != "")
            {
                TempData["error"] = error;
                return View("Index");
            }

            var model = new AddFoodViewModel
            {
                FoodItems = foodItemList
            };
            
            if (error != "") TempData["error"] = error;

            GetFormLists();
            return View(model);
        }

        [HttpPost]
        public IActionResult Add(AddFoodAndUserItemViewModel item)
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

            if (affectedRows == 0)
            {
                TempData["error"] = "Det gick inte att lägga till matvaran";
            }

            return RedirectToAction("Index");
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

            if (affectedRows == 0)
            {
                TempData["error"] = "Det gick inte att lägga till matvaran";
            }

            return RedirectToAction("Index");
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
        public IActionResult Delete(int id)
        {
            return View();
        }

    }
}