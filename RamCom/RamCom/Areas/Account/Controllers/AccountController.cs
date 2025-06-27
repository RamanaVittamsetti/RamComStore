using Business.Filters.Action;
using Business.Interfaces;
using Business.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using RamCom.Areas.Account.Models;
using RamCom.Interfaces;
using System.Runtime.CompilerServices;
using Serilog;

namespace RamCom.Areas.Account.Controllers
{
    [Area("Account")]
    [Route("Account")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;
        private readonly IRegisterViewModelBuilder _registerBuilder;
        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ILogger<AccountController> logger, IRegisterViewModelBuilder registerBuilder)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _registerBuilder = registerBuilder;
        }

        [Route("Login")]
        [BlockAuthenticated]
        [HttpGet]
        public ActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [Route("Login")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ModelState.AddModelError("validations_failed", "Invalid UserName or Password");
                    return View(model);
                }
                var user = await _userManager.FindByNameAsync(model.UserName);
                if (user != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index", "Home", new { area = "" });
                    }
                }
                else
                {
                    ModelState.AddModelError("invalid_credentials", "Invalid UserName or Password. Please try again.");
                }
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("internal_error", "Internal error occurred while processing the request. Please try again.");
                _logger.LogError(ex, ex.Message);
            }
               
            return View(model);

        }

        [Route("Logout")]
        [BlockAnonymous]
        [HttpGet]
        public async Task<ActionResult> SignOut()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        [Route("AccessDenied")]
        public ActionResult AccessDenied()
        {
            return View(new LoginViewModel());
        }

        [Route("Register")]
        [BlockAuthenticated]
        [HttpGet]
        public ActionResult Register()
        {
            var model = _registerBuilder.BuildViewModel();
            return View("RegisterOrUpdate", model);
        }

        [Route("UpdateDetails")]
        [BlockAnonymous]
        public async Task<ActionResult> UpdateDetails()
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            if (user != null)
            {
                var model = new RegisterViewModel
                {
                    UserName = user.UserName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    IsUpdateMode = true,
                    Role = user.Role
                };
                return View("RegisterOrUpdate", _registerBuilder.BuildViewModel(model));

            }
            _logger.LogWarning("User not registered, hence signing out");
            return RedirectToAction("SignOut");
        }

        [Route("Register")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (!model.IsUpdateMode) // Create
                {
                    // check user name
                    var user = await _userManager.FindByNameAsync(model.UserName);
                    if (user != null)
                    {
                        ModelState.AddModelError("", "User with this user name already exists.");
                        return View(_registerBuilder.BuildViewModel(model));
                    }

                    // check email
                    var userForPwd = await _userManager.FindByEmailAsync(model.Email);
                    if (userForPwd != null)
                    {
                        ModelState.AddModelError("", "User with this email already exists.");
                        return View(_registerBuilder.BuildViewModel(model));
                    }


                    var newUser = new ApplicationUser()
                    {
                        UserName = model.UserName,
                        Email = model.Email,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        PhoneNumber = model.PhoneNumber,
                        Role = model.Role
                    };
                    var result = await _userManager.CreateAsync(newUser, model.Password);
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(newUser, model.Role);

                        return RedirectToAction("Login");
                    }
                }
                else
                {
                    // Update
                    var user = await _userManager.FindByNameAsync(model.UserName);
                    if (user != null)
                    {
                        user.FirstName = model.FirstName;
                        user.LastName = model.LastName;
                        user.PhoneNumber = model.PhoneNumber;
                        var result = await _userManager.UpdateAsync(user);
                        if (result.Succeeded)
                        {
                            var passwordUpdate = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.Password);
                            await _signInManager.RefreshSignInAsync(user);
                            return RedirectToAction("Index", "Home", new { Area = "" });
                        }
                    }
                }

            }
            else
            {
                ModelState.AddModelError("", "Invalid Registration Details");
            }
            return View(_registerBuilder.BuildViewModel(model));
        }
    }
}
