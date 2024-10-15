using MatKollen.Controllers.Repositories;
using MatKollen.DAL.Repositories;
using MatKollen.Models;
using Microsoft.AspNetCore.Mvc;

namespace MatKollen.Controllers
{
    public class RecipeController : Controller
    {
        private readonly RecipeRepository _recipeRepository;
        private readonly FoodRepository _foodRepository;

        public RecipeController(RecipeRepository recipeRepository, FoodRepository foodRepository)
        {
            _recipeRepository = recipeRepository;
            _foodRepository = foodRepository;
        }

        //Recipe
        public IActionResult Index()
        {
            var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value);
            
            var recipeList = _recipeRepository.GetRecipes(out string error);
            return View(recipeList);
        }

        //Recipe/Details
        public IActionResult Details(int id)
        {
            var recipe = _recipeRepository.GetRecipe(id, out string error);

            int userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value);
            var userFoodItems = _foodRepository.GetUserFoodList(userId, out string listError);

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