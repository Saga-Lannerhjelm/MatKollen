using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MatKollen.Models;
using MatKollen.Controllers.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace MatKollen.Controllers;


// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-8.0
public interface IFoodService{
    public void AddUser();
}

public class FoodService : IFoodService{
    public void AddUser(){
        Console.Write("h"); //använder db
    }
}

public class FakeFoodService : IFoodService{
    public void AddUser(){
        Console.Write("h"); // använder inte db
    }
}


public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IFoodService _foodService;

    public HomeController(ILogger<HomeController> logger,FoodService foodService)
    {
        _logger = logger;
        _foodService = foodService;
    }

    [AllowAnonymous]
    public IActionResult Index()
    {
        var foodRep = new FoodRepository();
        // var categories = foodRep.GetData(out string error);
        // _foodService.AddUser();
        ViewBag.categories = "categories";
        return View();
    }

    public IActionResult Privacy()
    {
        var currentUser = HttpContext.User;
        var user = User;
        ViewBag.Username = currentUser.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
