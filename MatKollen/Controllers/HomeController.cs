using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MatKollen.Models;
using MatKollen.Controllers.Repositories;

namespace MatKollen.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        var foodRep = new FoodRepository();
        var categories = foodRep.GetData(out string error);

        ViewBag.categories = categories;
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
