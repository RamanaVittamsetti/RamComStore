using Business.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.EFContext
{
    public class ApplicationDBContext : IdentityDbContext<ApplicationUser>
    {
        private readonly IConfiguration _config;
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options, IConfiguration config) : base(options)
        {
            _config = config;
        }

        public DbSet<APIUser> APIUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); //Important for Identity configurations

            modelBuilder.Entity<APIUser>(entity => entity.HasData(new APIUser { Id=1, UserName = _config["APIUser:UserName"], Password = _config["APIUser:Password"] }));
        }
    }
}
