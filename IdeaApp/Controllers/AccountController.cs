using System.Text;
using System.Threading.Tasks;
using IdeaApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Text.Encodings.Web;
using System.Net;
using Microsoft.AspNetCore.Authentication;
namespace IdeaApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<AccountController> _logger;

        public AccountController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, IEmailSender emailSender, ILogger<AccountController> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _emailSender = emailSender;
            _logger = logger;
        }

        //landing page actions
        [HttpGet]
        public IActionResult LandingPage()
        {
            return View();
        }

        [HttpPost]
        public IActionResult LandingPage(string action)
        {
            if(action == "sign-up")
            {
                return RedirectToAction("Register");
            }
            else if(action == "log-in")
            {
                return RedirectToAction("Login");
            }
            return View();
        }

        //Get registration page
        public IActionResult Register()
        {
            return View();
        }

        //Post method for when a user register
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if(ModelState.IsValid)
            {
                //Create new user
                var user = new AppUser
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    UserName = model.Email,
                    Email = model.Email,
                    EmailConfirmed = false
                };

                //Create user in database
                var result = await _userManager.CreateAsync(user, model.Password);

                //if result = a success, sign in user
                if(result.Succeeded)
                {
                    //generate email confirmation token
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    //encode token into url friendly format
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                    //build confirmation link for confirm email page
                    var callbackUrl = Url.Action(
                        action: "ConfirmEmail",
                        controller: "Account",
                        values: new{userId = user.Id, code},
                        protocol: HttpContext.Request.Scheme
                    );

                    //send confirmation email to user
                    await _emailSender.SendEmailAsync
                    (
                        user.Email, 
                        "Confirm Your Account",
                        $"Hello {user.FirstName}, Please <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>click here</a> to confirm your account"
                    );

                    //Redirect based on confirmation requirement
                    if (_userManager.Options.SignIn.RequireConfirmedEmail)
                    {
                        //send user to register confirmation page
                        return RedirectToAction("RegisterConfirmation", new{email = model.Email});
                    }
                    else
                    {
                        //send user to homepage
                        await _signInManager.SignInAsync(user, isPersistent: true);
                        return RedirectToAction("Index", "Home");
                    }
                }

                //else if result fails print out all the errors
                foreach(var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, "Registration Failed Like Arsenal's Title Challenge. Try Again.");
                } 
            }
            //if modelstate wasnt valid redisplay the registration form
            return View(model);
        }

        //Login controller
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        //post method to handle login
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel user, string returnUrl = null)
        {
            //pass returnUrl to the view
            ViewData["ReturnUrl"] = returnUrl;

            //if user's data is valid
            if(ModelState.IsValid)
            {
                //find user
                var appUser = await _userManager.FindByEmailAsync(user.Email);

                //if user hasnt confirmed email send them back to the login page
                if (appUser != null && !appUser.EmailConfirmed)
                {
                    ModelState.AddModelError(string.Empty, "Someone hasn't confirmed their email. You MUST GO (like your president) to your email and look for a confirmation email from IdeaHub");
                    return View(user);
                }
                
                //log info on whether user clicked remember me
                _logger.LogInformation($"Remember Me: {user.RememberMe}");

                //sign them in
                var result = await _signInManager.PasswordSignInAsync(user.Email, user.Password, user.RememberMe, lockoutOnFailure:false);

                //if result is a success
                if(result.Succeeded)
                {
                    //log that user logged in
                    _logger.LogInformation("User logged in");

                    //redirect user to where they were on the site
                    if((!string.IsNullOrEmpty(returnUrl)) && Url.IsLocalUrl(returnUrl))
                    {
                        return LocalRedirect(returnUrl);
                    }
                    //otherwise redirect user to homepage
                    return RedirectToAction("Index", "Home");
                }

                //otherwise log error and send user back to login page
                ModelState.AddModelError(string.Empty, "Don't tell me you've forgotten your username/password!");
                return View(user); 
            }
            //if user's data is invalid send him back to login page
            foreach(var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine(error.ErrorMessage);
            }
            return View(user);
        }

        //Re
        [HttpGet]
        public async Task<IActionResult> RegisterConfirmation(string email, string returnUrl = null)
        {
            //if there's no valid email send them home
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Index", "Home");
            }

            //otherwise find user by email 
            var user = await _userManager.FindByEmailAsync(email);

            //if user is not found return user not found message
            if(user == null)
            {
                return NotFound($"Unable to load user with email {email}");
            }
        
            //otherwise pass first name to the view
            ViewData["FirstName"] = user.FirstName; 
            
            return View();  
        }

        //when user clicks link in email
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            //find the user
            var user = await _userManager.FindByIdAsync(userId);

            //log the user id just to confirm its the right one.
            _logger.LogInformation($"The user id is {userId}");

            if(user == null)
            {
                _logger.LogError("User is null");
                return RedirectToAction("LandingPage");
            }

            //Decode the token
            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));

            //log the decoded token to check if it matches the original one
            _logger.LogInformation($"The decoded token is {code}");

            //Confrim the user's email
            var result = await _userManager.ConfirmEmailAsync(user, code); 

            //log the token just to confirm its the right one
            _logger.LogInformation($"The token is {code}");

            //if result succeeds....
            if(result.Succeeded)
            {
                //set email confirmed to true
                user.EmailConfirmed = true;

                //save the changes
                var updateResult = await _userManager.UpdateAsync(user);

                //if changes fail to be saved ...
                if (!updateResult.Succeeded)
                {   
                    //add each error to the model state and log it too
                    foreach (var error in updateResult.Errors)
                    {
                        ModelState.AddModelError(error.Code, error.Description);
                        _logger.LogError($"Error Code {error.Code}, Error Description {error.Description}");
                    }
                    //send user to landing page
                    return RedirectToAction("LandingPage");
                }
                //otherwise if changes are saved send user to login page
                return RedirectToAction("Login");
            };
            
            //if result doesnt succeed log why & send user to landing page
            foreach(var error in result.Errors)
            {
                _logger.LogError($"Error Code: {error.Code}, Error Description: {error.Description}");
                ModelState.AddModelError(error.Code, error.Description);
            }
            return RedirectToAction("LandingPage");

        }

        //what happens when user clicks logout
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            //sign user out
            await _signInManager.SignOutAsync();

            //log message saying user signed out
            _logger.LogInformation("User signed out");

            //redirect user to login page
            return RedirectToAction("LandingPage");
        }
    }


}
