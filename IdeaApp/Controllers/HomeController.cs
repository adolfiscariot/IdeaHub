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

    public IActionResult Index(Idea idea)
    {
       var no_of_ideas = _context.Ideas.Count();
       ViewData["no_of_ideas"] = no_of_ideas; 

        //REVIEW THIS CODE!!!!!!-------------------------

        // Total number of contributors (unique authors)
        var no_of_contributors = _context.Ideas.Select(i => i.Author).Distinct().Count();
        ViewData["no_of_contributors"] = no_of_contributors;

        // Find the top contributor
        var topContributor = _context.Ideas
            .GroupBy(i => i.Author) // Group by author
            .Select(g => new 
            { 
                Author = g.Key, // Author name
                IdeaCount = g.Count() // Number of ideas for this author
            })
            .OrderByDescending(g => g.IdeaCount) // Sort by idea count (descending)
            .FirstOrDefault(); // Get the top contributor

        // Pass the top contributor to the view
        if (topContributor != null)
        {
            ViewData["topContributorName"] = topContributor.Author;
            ViewData["topContributorIdeas"] = topContributor.IdeaCount;
        }
        else
        {
            ViewData["topContributorName"] = "No contributors yet";
            ViewData["topContributorIdeas"] = 0;
        }

        //--------------------------------------
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
