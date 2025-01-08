using System;
using IdeaApp.Models;
using Microsoft.AspNetCore.Mvc;
namespace IdeaApp.Controllers;

using System.Security.Claims;
using IdeaApp.Data;
using IdeaApp.Helpers;
using IdeaApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Web;

[Authorize]
public class IdeasController : Controller
{
    private readonly ILogger<IdeasController> _logger;
    private readonly IdeaappDbContext _context;

    public IdeasController(ILogger<IdeasController> logger, IdeaappDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    [HttpGet]
    public IActionResult Index()
    {
        //fetch ideas from database
        var ideas = _context.Ideas.ToList();
        return View(ideas);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new IdeaViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Create(IdeaViewModel viewModel)
    {
        //if user data doesn't meet requirements e.g. length, data type etc
        if (!ModelState.IsValid)
        {
            foreach(var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                _logger.LogError(error.ErrorMessage);
            }
            return View(viewModel);
        }

        //fetch user's id
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation($"user id is {userId}");

        //check if userId is null
        if(string.IsNullOrEmpty(userId))
        {
            ModelState.AddModelError(string.Empty, "Couldn't retrieve user id" );
            return Unauthorized();
        }

        //fetch user's name
        var author = await _context.Users.FindAsync(userId);
        _logger.LogInformation($"the author is {author}");

        //check if author is null
        if(author == null)
        {
            _logger.LogError("Idea's author not found in database");
            return RedirectToAction("Register", "Account");
        }

        //if author & userId aren't null add new idea
        var idea = new Idea 
        {
            Name = viewModel.Name,
            Content = viewModel.Content,
            UserId = userId,
            DateWritten = DateOnly.FromDateTime(DateTime.UtcNow),
            Author = author
        };

        //add idea to Ideas & save
        _context.Ideas.Add(idea);
        await _context.SaveChangesAsync();
        _logger.LogInformation("changes saved");

        //redirect user back to idea list
        return RedirectToAction("Index");

    }

    //vote increment
    [HttpPost]
    public async Task<IActionResult> Vote(int ideaId)
    {
        //fetch user
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation($"user id is {userId}");

        //if user id isnt found return error
        if(string.IsNullOrEmpty(userId))
        {
            _logger.LogError("User id not found");
            return Unauthorized();
        }

        //otherwise fetch idea
        var idea = await _context.Ideas.FindAsync(ideaId);
        _logger.LogInformation("idea found");

        //if idea is null return error
        if(idea == null)
        {
            _logger.LogError("Idea not found");
            return NotFound();
        }

        //convert the voters string to a list
        var voters = VoterHelper.ToList(idea.Voters);
        _logger.LogInformation("voters found");

        //if user has voted, remove vote 
        if (voters.Contains(userId))
        {
            voters.Remove(userId);
            idea.VoteCount --;
        }
        //otherwise add vote
        else
        {
            voters.Add(userId);
            idea.VoteCount ++;
            
        }

        //convert the list back to a string for storage
        idea.Voters = VoterHelper.ToCommaSeparatedString(voters);

        //save changes
        await _context.SaveChangesAsync();

        //return vote count
        return RedirectToAction("Index");
    }
}
