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
                TempData["error"] = "Gick inte att hämta listan";
            }

            ViewData["Title"] = groceryItems.Count != 0 ? groceryItems[0].ListName : "Inköpslista";

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
        public IActionResult DeleteItem(int id)
        {
            return View();
        }

        [HttpPost]
        public IActionResult MarkAsComplete(int id)
        {
            return View();
        }
    }
}