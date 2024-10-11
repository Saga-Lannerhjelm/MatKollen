using MatKollen.Models;
using Microsoft.AspNetCore.Mvc;

namespace MatKollen.Controllers
{
    public class RecipeController : Controller
    {
        //Recipe
        public IActionResult Index()
        {
            return View();
        }

        //Recipe/Details
        public IActionResult Details(int id)
        {
            return View();
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