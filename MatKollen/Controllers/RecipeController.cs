using MatKollen.Controllers.Repositories;
using MatKollen.DAL.Repositories;
using MatKollen.Models;
using Microsoft.AspNetCore.Mvc;

namespace MatKollen.Controllers
{
    public class RecipeController : Controller
    {
        //Recipe
        public IActionResult Index()
        {
            var recRep = new RecipeRepository();
            var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value);
            
            var recipeList = recRep.GetRecipes(out string error);
            return View(recipeList);
        }

        //Recipe/Details
        public IActionResult Details(int id)
        {
            var recRep = new RecipeRepository();
            var recipe = recRep.GetRecipe(id, out string error);


            var foodRep = new FoodRepository();
            int userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value);
            var userFoodItems = foodRep.GetUserFoodList(userId, out string listError);

            foreach(var item in recipe.Ingredients)
            {
                if (userFoodItems.Find(food => food.FoodItemDetails.FoodItemId == item.IngredientDetails.FoodItemId) != null)
                {
                    item.UserHasIngredient = true;
                    
                    // Lägg allt som blir falskt i en egen variabel så att det kan användas till att enkelt lägg till sakerna i inköpslista
                }
            }

            return View(recipe);
        }

        //Recipe/Saved
        public IActionResult Saved()
        {
            return View();
        }

        //Recipe/Create
        [HttpGet]       
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]       
        public IActionResult Create(Recipe recipe)
        {
            return View();
        }

        [HttpGet]
        //Recipe/Edit
        public IActionResult Edit(int id)
        {
            return View();
        }

        [HttpPost]
        //Recipe/Edit
        public IActionResult Edit(Recipe recipe)
        {
            return View();
        }

        [HttpPost]
        //Recipe/Delete
        public IActionResult Delete(int id)
        {
            return View();
        }


    }
}