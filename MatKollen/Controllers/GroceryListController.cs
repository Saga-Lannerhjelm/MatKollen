using MatKollen.Controllers.Repositories;
using MatKollen.DAL.Repositories;
using MatKollen.Extensions;
using MatKollen.Models;
using Microsoft.AspNetCore.Mvc;
using Mysqlx;

namespace MatKollen.Controllers
{
    public class GroceryListController : Controller
    {
        private readonly GroceryListRepository _groceryListRepository;
        private readonly FoodRepository _foodRepository;

        public GroceryListController(GroceryListRepository groceryListRepository, FoodRepository foodRepository)
        {
            _groceryListRepository = groceryListRepository;
            _foodRepository = foodRepository;
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
        public IActionResult AddFoodItems()
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

            return RedirectToAction("index");
        }

        //GroceryList/AddItem
        [HttpGet]
        public IActionResult AddItem()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddItem(FoodItem item)
        {
            return View();
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