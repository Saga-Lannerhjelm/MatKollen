using MatKollen.Models;
using Microsoft.AspNetCore.Mvc;

namespace MatKollen.Controllers
{
    public class GroceryListController : Controller
    {
        //GroceryList
        public IActionResult Index()
        {
            return View();
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