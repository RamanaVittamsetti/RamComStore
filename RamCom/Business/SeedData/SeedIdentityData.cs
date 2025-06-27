using Business.Extensions;
using Business.Interfaces;
using Business.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Serilog;
using Azure.Identity;

namespace Business.SeedData
{
    public static class SeedIdentityData
    {
        public static async Task SeedData(IServiceProvider serviceProvider)
        {
            var _userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var _roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var _configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var _webHostenv = serviceProvider.GetRequiredService<IWebHostEnvironment>();
            var _xmlCacheHelper = serviceProvider.GetRequiredService<IXmlCacheHelper>();
            var _logger = serviceProvider.GetRequiredService<ILogger>();

            try
            {
                //Add Roles
                var fullPath = Path.Combine(_webHostenv.WebRootPath, _configuration["XmlFiles:UserRole"]);
                XDocument document = _xmlCacheHelper.RetriveXmlValues("UserRoles", fullPath);
                if (document != null)
                {
                    foreach (var element in document.Descendants("Role"))
                    {
                        if (element.Value.IsNotNullOrWhiteSpace() && !await _roleManager.RoleExistsAsync(element.Value))
                        {
                            await _roleManager.CreateAsync(new IdentityRole(element.Value));
                        }
                    }
                }

                //Add Admin Users
                string userName = _configuration["SiteAdmin:UserName"];
                var user = await _userManager.FindByNameAsync(userName);
                if (user == null)
                {
                    string role = _configuration["SiteAdmin:Role"];
                    var appUser = new ApplicationUser
                    {
                        UserName = userName,
                        Email = _configuration["SiteAdmin:Email"],
                        FirstName = userName,
                        LastName = userName,
                        Role = role

                    };
                    var result = await _userManager.CreateAsync(appUser, _configuration["SiteAdmin:Password"]);
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(appUser, role);
                        await _userManager.AddClaimAsync(appUser, new System.Security.Claims.Claim(ClaimTypes.Role, role));
                    }
                }

            }

            catch(Exception ex)
            {
                _logger.Error(ex, ex.Message);
            }

        }
    }
}
