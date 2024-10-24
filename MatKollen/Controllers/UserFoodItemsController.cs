using MatKollen.Controllers.Repositories;
using MatKollen.DAL.Repositories;
using MatKollen.Models;
using MatKollen.Services;
using Microsoft.AspNetCore.Mvc;

namespace MatKollen.Controllers
{
    public class UserFoodItemsController : Controller
    {
        private readonly FoodRepository _foodRepository;
        private readonly UnitsRepository _unitRepository;
        private readonly FoodCategoriesRepository _foodCategoryRepository;
        private readonly ConvertQuantityHandler _convertQuantityHandler;

        public UserFoodItemsController(FoodRepository foodRepository, UnitsRepository unitsRepository, FoodCategoriesRepository foodCategoriesRepository, ConvertQuantityHandler convertQuantityHandler)
        {
            _foodRepository = foodRepository;
            _unitRepository = unitsRepository;
            _foodCategoryRepository = foodCategoriesRepository;
            _convertQuantityHandler = convertQuantityHandler;
        }

        public IActionResult Index(string showAccordionName)
        {
            int userId = UserHelper.GetUserId(User);
            var foodList = _foodRepository.GetUserFoodList(userId, out string error);
            
            if (error != "") TempData["error"] = error;

            // sort lists by expiration date
            for (int i = 0; i < foodList?.Count; i++)
            {
                foodList[i].UserFoodItems.Sort((a, b) => a.FoodDetails.ExpirationDate.CompareTo(b.FoodDetails.ExpirationDate));
            }
            foodList.Sort((a,b) => a.UserFoodItems[0].FoodDetails.ExpirationDate.CompareTo(b.UserFoodItems[0].FoodDetails.ExpirationDate));

            ViewBag.showAccordionName = showAccordionName;

            return View("Index", foodList);
        }

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
            var unitList = _unitRepository.GetUnits(out string unitsError);
            if (unitsError != "")
            {
                TempData["error"] = unitsError;
                return View(item);
            }
            double multiplier = unitList.Find(m => m.Id == item.UnitId).Multiplier;

            //Get user id
            int userId = UserHelper.GetUserId(User);
            item.UserId = userId;

            // Recalculate quantity
            item.Quantity = _convertQuantityHandler.ConverToLiterOrKg(item.Quantity, multiplier);

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
            var unitList = _unitRepository.GetUnits(out string unitsError);
            List<FoodItem>? foodItemList = _foodRepository.GetFoodItems(out string error);

            if (unitsError != "" || error != "") TempData["error"] = unitsError + " " + error;

            ViewData["units"] = unitList;
            ViewData["foodItems"] = foodItemList;
        }

        // Update

        [HttpPost]
        public IActionResult IncreaseQuantity(int id, double unitMultiplier, string unit, string showAccordionName)
        {
            decimal incrementNr = Convert.ToDecimal(unit != "kg" && unit != "L" ? 1 / unitMultiplier : 0.1);
            var newQuantity = _foodRepository.UpdateQuantity(id, incrementNr, out string error);
            if (error != "") TempData["error"] = error;
            return new JsonResult(new { success = true, newQuantity });
        }

        [HttpPost]
        public IActionResult DecreaseQuantity(int id, double unitMultiplier, decimal quantity, string unit, string showAccordionName, bool canDelete)
        {
            if (quantity > 0)
            {
                decimal decreaseNr = Convert.ToDecimal((unit != "kg" && unit != "L" ? 1 / unitMultiplier : 0.1) * -1);
                var affectedRows = _foodRepository.UpdateQuantity(id, decreaseNr, out string error);
                if (affectedRows == 0) TempData["error"] = "Det gick inte att ändra antalet";
                if (error != "") TempData["error"] = error;
            }
            else if (canDelete)
            {
                 var affectedRows = _foodRepository.DeleteFoodItem(id, out string error);

            if (affectedRows == 0) TempData["error"] = "Det gick inte att ta bort varan";
            if (error != "") TempData["error"] = error;
            }
            return RedirectToAction("Index", new {showAccordionName});
        }

        [HttpPost]
        public IActionResult AddExpirationDate(int id, DateOnly expirationDate)
        {
             var affectedRows = _foodRepository.UpdateExpirationDate(id, expirationDate, out string error);
            if (affectedRows == 0) TempData["error"] = "Gick inte att lägga till datumet datumet";
            if (error != "") TempData["error"] = error;
            
            return RedirectToAction("Index");
        }


        // Delete

        [HttpPost]
        public IActionResult Delete(int foodId, int userId, string type)
        {
            var affectedRows = _foodRepository.DeleteAllOfFoodItem(foodId, userId, type, out string error);

            if (error != "") TempData["error"] = error;
            if (affectedRows == 0) TempData["error"] = "Det gick inte att ta bort varan";
            return RedirectToAction("index");
        }
        
    }
}