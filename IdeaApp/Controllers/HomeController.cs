using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using IdeaApp.Models;
using Microsoft.AspNetCore.Authorization;

namespace IdeaApp.Controllers;
using IdeaApp.Data;
[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IdeaappDbContext _context;

    public HomeController(ILogger<HomeController> logger, IdeaappDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
       var no_of_ideas = _context.Ideas.Count();
       ViewData["no_of_ideas"] = no_of_ideas; 
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
