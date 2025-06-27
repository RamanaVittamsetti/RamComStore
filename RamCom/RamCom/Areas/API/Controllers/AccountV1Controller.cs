using Business.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;
using RamCom.Areas.Account.Controllers;
using RamCom.Areas.Account.Models;
using RamCom.Areas.API.Data;

namespace RamCom.Areas.API.Controllers
{
    [Route("api/v1/account")]
    [Authorize(AuthenticationSchemes = "JwtBearer")]
    [Area("API")]
    [ApiController]
    public class AccountV1Controller : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;

        public AccountV1Controller(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new LoginResponse { Success = false, Message = "Invalid data" });

            var user = await _userManager.FindByNameAsync(request.UserName);

            if (user == null)
                return Unauthorized(new LoginResponse { Success = false, Message = "User not found" });

            // Verify password
            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            

            if (!result.Succeeded)
            {
                return Unauthorized(new LoginResponse { Success = false, Message = "Invalid password" });
            }

            return Ok(new LoginResponse
            {
                Success = true,
                Message = "Login successful",
                Role = user.Role
            });
        }
    }
}
