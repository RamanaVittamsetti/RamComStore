using Business.Extensions;
using DataAccess.EFContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Text;

namespace RamCom.Areas.API.Controllers
{
    [ApiController]
    [Area("API")]
    [Route("api/token")]
    public class JwtAuthTokenController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<JwtAuthTokenController> _logger;
        private readonly ApplicationDBContext _dbContext;

        public JwtAuthTokenController(IConfiguration configuration, ILogger<JwtAuthTokenController> logger, ApplicationDBContext dbContext)
        {
            _configuration = configuration;
            _logger = logger;
            _dbContext = dbContext;
        }

        [Route("")]
        [HttpPost]
        [AllowAnonymous]
        public IActionResult GetToken()
        {
            string authHeader = Request.Headers["Authorization"];
            if (!authHeader.IsNotNullOrWhiteSpace() || !authHeader.StartsWith("Basic "))
            {
                _logger.LogError("Authorization header is missing or invalid");
                return Unauthorized(new
                {
                    Message = "Authorization header is missing or invalid"
                });
            }
                
            string encodedCredentails = authHeader.Split(" ")[1];
            string decodedCredentails = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentails));
            string[] credentails = decodedCredentails.Split(":");
            string userName = credentails[0];
            string password = credentails[1];
            if(_dbContext.APIUsers != null && _dbContext.APIUsers.FirstOrDefault(x=>x.UserName== userName && x.Password== password) !=null)
            {
                var token = generateJwtToken(userName);
                return Ok(new { token });
            }
            _logger.LogError("Invalid username or password");
            return Unauthorized(new { Message = "Invalid username or password" });

        }

        private string generateJwtToken(string userName)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, userName)
            };
            var token = new JwtSecurityToken
                (
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                signingCredentials: signingCredentials,
                expires: DateTime.Now.AddMinutes(60)
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
           
        }

    }
}
