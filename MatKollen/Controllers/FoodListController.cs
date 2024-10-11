using MatKollen.Models;
using Microsoft.AspNetCore.Mvc;

namespace MatKollen.Controllers
{
    public class FoodListController : Controller
    {
        //FoodItems
        public IActionResult Index()
        {
            return View();
        }

        //FoodItems/Add
        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Add(FoodItem item)
        {
            return View();
        }

        //FoodItems/Edit
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

        //FoodItems
        [HttpPost]
        public IActionResult Delete(int id)
        {
            return View();
        }

    }
}