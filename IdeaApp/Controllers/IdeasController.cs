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
using Microsoft.AspNetCore.Identity;

[Authorize]
public class IdeasController : Controller
{
    private readonly ILogger<IdeasController> _logger;
    private readonly IdeaappDbContext _context;
    private readonly UserManager<AppUser> _userManager;

    public IdeasController(ILogger<IdeasController> logger, IdeaappDbContext context, UserManager<AppUser> userManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public IActionResult Index()
    {
        ViewData["email"] = User.Identity?.Name;
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

        //if user has voted, remove vote and decrement vote count
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
        //fetch user id
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        ViewData["userId"] = userId;

        //fetch idea
        var idea = await _context.Ideas.FindAsync(ideaId); 

        //if idea is null log error
        if(idea == null) 
        {
           _logger.LogError($"Idea with ID {ideaId} not found");
           return NotFound(); 
        }

        //check if logged in user is idea author
        if (idea.UserId != userId){
            _logger.LogWarning($"Unauthorized edit by user id: {userId}");
            return Forbid();
        }

        //otherwise show idea
        return View(idea);

    }

    //commit the changes to the database
    [Authorize]
    [HttpPut("{ideaId}")]
    public async Task<IActionResult> ConfirmEdit(int ideaId, [Bind("Name, Content")] IdeaViewModel viewModel)
    {   
        //fetch logged in user id
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        ViewData["userId"] = userId;
        _logger.LogInformation($"USER ID = {userId}");

        //fetch idea
        var idea = await _context.Ideas.FindAsync(ideaId);

        //check if idea is null
        if(idea == null)
        {
            _logger.LogError($"Idea with ID {ideaId} not found");
            return NotFound();
        }

        //check if logged in user is idea author
        if (idea.UserId != userId){
            _logger.LogWarning($"Unauthorized edit by user id: {userId}");
            return Forbid();
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
        //fetch user id
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        ViewData["userId"] = userId;

        //find idea based on primary key ideaId
        var idea = await _context.Ideas.FindAsync(ideaId);   

        //if idea doesn't exist print an error message
        if(idea == null) 
        {
            _logger.LogError($"idea with id {ideaId} not found");
            return NotFound();
        }

        //check if logged in user is idea author
        if (idea.UserId != userId){
            _logger.LogWarning($"Unauthorized delete attempt by user id: {userId}");
            return Forbid();
        }

        //if it exists display it on the screen
        return View(idea);        
    }

    //make deletion permanent
    [HttpPost]
    public async Task<IActionResult> ConfirmDelete(int ideaId)
    {
        //fetch idea
        var idea = await _context.Ideas.FindAsync(ideaId);

        //check if idea exists
        if(idea == null)
        {
            _logger.LogError($"idea with id {ideaId} doesn't exist");
            return NotFound();
        }

        //fetch user id and create view data for it
        var userId = _userManager.GetUserId(User);
        ViewData["UserId"] = userId;


        //VERIFY IF THIS PIECE OF CODE IS USEFUL OR IT SHOULD BE DELETED!!!
        //check if person deleting is the author
        if (userId != idea.UserId)
        {
            _logger.LogWarning($"Unauthorized attempted delete by user id: {userId}");
            return Forbid();
        }

        //otherwise remove idea from database
        _context.Ideas.Remove(idea);

        //save changes
        await _context.SaveChangesAsync();

        //redirect user to idea list
        return RedirectToAction("Index");
    }
}
