using IdeaApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdeaApp.Controllers;

//create admin controller class based on controller class
public class AdminController : Controller {
    
    //create role manager object
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<AdminController> _logger;
    public AdminController(RoleManager<IdentityRole> roleManager, ILogger<AdminController> logger) {
        _roleManager = roleManager;
        _logger = logger;
    }

    //create new role
    [HttpGet]
    public IActionResult CreateRole() {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateRole(CreateRoleViewModel roleModel) {
        if (ModelState.IsValid) {
             //check if role exists
            bool roleExists = await _roleManager.RoleExistsAsync(roleModel?.RoleName);

            //if it exists return error
            if(roleExists) {
                ModelState.AddModelError(string.Empty, "Role Already Exists");
            }

            //otherwise create the role
            IdentityRole identityRole = new IdentityRole {
                Name = roleModel?.RoleName
            };
            
            //save the role
            IdentityResult result  = await _roleManager.CreateAsync(identityRole);

            //if save succeeds send user to homepage
            if (result.Succeeded) {
                return RedirectToAction("Index", "Home");
            }
            
            //else log errors
            else {
                foreach(IdentityError error in result.Errors) {
                    _logger.LogError(error.Description);
                }
            }

        }
        return View(roleModel);
    }

    //list the roles
    public async Task<IActionResult> ListRoles() {
        List<IdentityRole> roles = await _roleManager.Roles.ToListAsync();
        return View(roles);
   }
} 