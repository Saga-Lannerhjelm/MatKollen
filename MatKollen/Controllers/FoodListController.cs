using MatKollen.Controllers.Repositories;
using MatKollen.Models;
using MatKollen.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Mysqlx;

namespace MatKollen.Controllers
{
    public class FoodListController : Controller
    {
        private readonly FoodRepository _foodRepository;

        public FoodListController(FoodRepository foodRepository)
        {
            _foodRepository = foodRepository;
        }
        
        //FoodItems
        public IActionResult Index()
        {
            var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value);
            
            var foodList = _foodRepository.GetUserFoodList(userId, out string error);
            return View(foodList);
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