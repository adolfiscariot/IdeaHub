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

        if (topContributor != null)
        {
            _logger.LogInformation($"Top Contributor: {topContributor.Author}");

            // Check if Author is null before accessing Email
            if (topContributor.Author != null)
            {
                var email = topContributor.Author.Email;
                if (email == null)
                {
                    _logger.LogError("Email is null");
                }
                else
                {
                    _logger.LogInformation($"Email is {email}");
                }
                ViewData["email"] = email;
            }
            else
            {
                _logger.LogError("Author is null");
                ViewData["email"] = "No author found";
            }

            // Pass top contributor to view
            ViewData["topContributor"] = topContributor.Author.FirstName + " " + topContributor.Author.LastName;
            ViewData["topContributorIdeas"] = topContributor.IdeaCount;
        }
        else
        {
            _logger.LogInformation("No contributors yet");
            ViewData["topContributor"] = "No contributors yet";
            ViewData["topContributorIdeas"] = 0;
            ViewData["email"] = "No email found";
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
