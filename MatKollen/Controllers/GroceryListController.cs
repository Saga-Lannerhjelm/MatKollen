using MatKollen.Controllers.Repositories;
using MatKollen.DAL.Repositories;
using MatKollen.Extensions;
using MatKollen.Models;
using MatKollen.Services;
using Microsoft.AspNetCore.Mvc;
using Mysqlx;

namespace MatKollen.Controllers
{
    public class GroceryListController : Controller
    {
        private readonly GroceryListRepository _groceryListRepository;
        private readonly FoodRepository _foodRepository;
        private readonly GetListsRepository _getListRepository;
        private readonly ConvertQuantityHandler _convertQuantityHandler;

        public GroceryListController
        (
            GroceryListRepository groceryListRepository, 
            FoodRepository foodRepository, 
            GetListsRepository getListsRepository, 
            ConvertQuantityHandler convertQuantityHandler
        )
        {
            _groceryListRepository = groceryListRepository;
            _foodRepository = foodRepository;
            _getListRepository = getListsRepository;
            _convertQuantityHandler = convertQuantityHandler;
        }

        //GroceryList
        public IActionResult Index()
        {
            int userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "id").Value);
            var groceryItems = _groceryListRepository.GetGroceryList(userId, out string error);

            if (error != "")
            {
                TempData["error"] = error;
            }

            return View(groceryItems);
        }

        [HttpPost]
        public IActionResult AddCompletedItemsToUserInventory()
        {
            int userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "id").Value);
            var foodItems = _groceryListRepository.GetCompletedItems(userId, out string error);

            if (error != "")
            {
                TempData["error"] = error;
            }

            if (foodItems != null)
            {
                foreach (var food in foodItems)
                {
                    var userFoodItem = new UserFoodItem()
                    {
                        Quantity = food.Quantity,
                        UserId = userId,
                        ExpirationDate = new DateOnly(),
                        FoodItemId = food.FoodItemId,
                        UnitId = food.UnitId
                    };
                    
                    var affectedRows = _foodRepository.AddFoodItem(userFoodItem, out string insertError);

                    if (insertError != "")
                    {
                        TempData["error"] = error;
                    }

                    if (affectedRows > 0)
                    {
                        _groceryListRepository.Delete(food.Id, out string deleteError);
                        if (deleteError != "")
                        {
                            TempData["error"] = error;
                        }
                    }
                }
            }

            return RedirectToAction("index", "FoodList");
        }

        //GroceryList/AddItem
        [HttpGet]
        public IActionResult AddItemToGroceryList()
        {
            var foodItem = new ListFoodItem();
            GetLists();
            return View(foodItem);
        }

        [HttpPost]
        public IActionResult AddItemToGroceryList(ListFoodItem item)
        {
            if (!ModelState.IsValid)
            {
                GetLists();
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

            // Recalculate quantity
            item.Quantity = _convertQuantityHandler.ConverToLiterOrKg(item.Quantity, multiplier);

            // Insert values in database
            int affectedRows = _groceryListRepository.InsertFoodItemInList(item, userId, out string error);

            if (error != "")
            {
                TempData["error"] = "Det gick inte att lägga till matvaran:" + error;
            }

            if (affectedRows == 0) TempData["error"] = "Det gick inte att lägga till matvaran";
            else TempData["success"] = "Matvara tillagd!";

            return RedirectToAction("Index");
        }

        private void GetLists()
        {
            List<MeasurementUnit> unitList = _getListRepository.GetUnits(out string unitsError);
            List<FoodItem>? foodItemList = _foodRepository.GetFoodItems(out string error);

            if (unitsError != "" || error != "") TempData["error"] = unitsError + " " + error;

            ViewData["units"] = unitList;
            ViewData["foodItems"] = foodItemList;
        }

         [HttpPost]
        public IActionResult AddFromRecipe(List<int> checkedItems)
        {
            int userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "id").Value);
            var groceryListItems = HttpContext.Session.GetObject<List<ListFoodItem>>("groceryList");
            var filteredGroceryItems = groceryListItems.Where(g => !checkedItems.Contains(g.FoodItemId)).ToList();

            foreach (var item in filteredGroceryItems)
            {
                int rowsAffected = _groceryListRepository.InsertOrUpdateFoodItems(item, userId, out string error);
                if (error != "")
                {
                    TempData["error"] = error;
                    break;
                }
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var rowsAffected = _groceryListRepository.Delete(id, out string error);
            if (error != "")
            {
                TempData["error"] = error;
            }
            if (rowsAffected == 0)
            {
                TempData["error"] = "Gick inte att markera varan som köpt";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult MarkAsComplete(int id, bool completed)
        {
            completed = !completed;
            var rowsAffected = _groceryListRepository.UpdateCompletedState(id, completed, out string error);
            if (error != "")
            {
                TempData["error"] = error;
            }
            if (rowsAffected == 0)
            {
                TempData["error"] = "Gick inte att markera varan som köpt";
            }

            return RedirectToAction("Index");
        }
    }
}