using MatKollen.Controllers.Repositories;
using MatKollen.Models;
using MatKollen.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Mysqlx;

namespace MatKollen.Controllers
{
    public class FoodListController : Controller
    {
        //FoodItems
        public IActionResult Index()
        {
            var foodRep = new FoodRepository();
            var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value);
            
            var foodList = foodRep.GetUserFoodList(userId, out string error);
            // var foodList = new List<UserFoodItemViewModel>();
            // for (int i = 0; i < fetchedList?.Count; i++)
            // {
            //     IEnumerable<UserFoodItemViewModel> countOcurrences = fetchedList.Where(item => item.FoodItem == fetchedList[i].FoodItem);
                
            //     if (foodList.Find(item => item.FoodItem.Contains(fetchedList[i].FoodItem)) == null)
            //     {
            //         var item = new UserFoodItemViewModel()
            //         {
            //             FoodItem = fetchedList[i].FoodItem,
            //             Category = fetchedList[i].Category,
            //             Amount = countOcurrences.Count()
            //         };
            //         foodList.Add(item);
            //     }
            // }
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