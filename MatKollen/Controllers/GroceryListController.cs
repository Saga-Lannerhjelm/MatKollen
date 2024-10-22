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

        public GroceryListController(GroceryListRepository groceryListRepository)
        {
            _groceryListRepository = groceryListRepository;
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