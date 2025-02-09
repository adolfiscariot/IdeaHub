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
        //show total number of ideas
       var no_of_ideas = _context.Ideas.Count();
       ViewData["no_of_ideas"] = no_of_ideas; 

        // Total number of contributors (unique authors)
       var no_of_contributors = _context.Ideas.Select(i => i.Author).Distinct().Count();
       ViewData["no_of_contributors"] = no_of_contributors;

       //top contributor
       var topContributor = _context.Ideas
       .GroupBy(i => i.Author)
       .Select(g => new
       {
            Author = g.Key,
            IdeaCount = g.Count()
       })
       .OrderByDescending(g => g.IdeaCount)
       .FirstOrDefault();

        _logger.LogInformation($"....: {topContributor}");
        var email =topContributor.Author.Email;
        if (email == null)
        {
            _logger.LogError("email is null");
        }
        else
        {
            _logger.LogInformation($"email is {email}");
        }
        ViewData["email"] = email;
        //pass top contributor to view
        if (topContributor != null)
        {
            _logger.LogInformation($"The top contributor is {topContributor.Author}");
            ViewData["topContributor"] = topContributor.Author.FirstName + " " + topContributor.Author.LastName;
            ViewData["topContributorIdeas"] = topContributor.IdeaCount;
        }
        
        else{
            ViewData["topContributor"] = "No contributors yet";
            ViewData["topContributorIdeas"] = 0;   
        }

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
