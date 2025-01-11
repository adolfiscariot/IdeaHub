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
        var ideas = _context.Ideas.OrderByDescending(i => i.VoteCount).ToList();
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

    //edit an idea
    [HttpGet("Ideas/Edit/{ideaId}")]
    public async Task<IActionResult> Edit(int ideaId)
    {
        var idea = await _context.Ideas.FindAsync(ideaId); 
        if(idea == null)
        {
           _logger.LogError($"Idea with ID {ideaId} not found");
           return NotFound(); 
        }
        return View(idea);

    }

    //commit the changes to the database
    [HttpPost]
    public async Task<IActionResult> ConfirmEdit(int ideaId, [Bind("Name, Content")] IdeaViewModel viewModel)
    {   
        //fetch idea
        var idea = await _context.Ideas.FindAsync(ideaId);

        //check if idea is null
        if(idea == null)
        {
            _logger.LogError($"Idea with ID {ideaId} not found");
            return NotFound();
        }

        //bind edited name & content to the name & content properties in the form
        idea.Name = viewModel.Name; 
        idea.Content = viewModel.Content;

        //save changes and send user back to idea list page
        await _context.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    [HttpGet("Ideas/Delete/{ideaId}")]
    public async Task<IActionResult> Delete(int ideaId)
    {
        //find idea
        var idea = await _context.Ideas.FindAsync(ideaId);   

        //check if idea exists
        if(idea == null)
        {
            _logger.LogError($"idea with id {ideaId} not found");
            return NotFound();
        }

        //if it exists display it on the screen
        return View(idea);        
    }

    //make deletion permanent
    [HttpPost]
    public async Task<IActionResult> ConfirmDelete(int ideaId)
    {
        var idea = await _context.Ideas.FindAsync(ideaId);

        //check if idea exists
        if(idea == null)
        {
            _logger.LogError($"idea with id {ideaId} doesn't exist");
            return NotFound();
        }

        //otherwise remove idea from database
        _context.Ideas.Remove(idea);

        //save changes
        await _context.SaveChangesAsync();

        //redirect user to idea list
        return RedirectToAction("Index");
    }
}
