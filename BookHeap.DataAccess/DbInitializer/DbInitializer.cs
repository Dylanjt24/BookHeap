using BookHeap.DataAccess.Repository;
using BookHeap.DataAccess.Repository.IRepository;
using BookHeap.Models;
using BookHeap.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookHeap.DataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        // Used for getting values from user secrets
        private readonly IConfiguration _configuration;

        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;

        public DbInitializer(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext db,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
            _configuration = configuration;
        }

        public void Initialize()
        {
            // Apply migrations if not already applied
            try
            {
                if (_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }
            }
            catch (Exception ex)
            {

            }
            // Create roles if not already created
            if (!_roleManager.RoleExistsAsync(SD.Role_Admin).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_User_Indi)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_User_Comp)).GetAwaiter().GetResult();

                // Also create admin if roles are not created
                _userManager.CreateAsync(new ApplicationUser
                {
                    UserName = _configuration["Initializer:Email"],
                    Email = _configuration["Initializer:Email"],
                    Name = "Admin User",
                    PhoneNumber = "5551112345",
                    StreetAddress = "123 Test Blvd",
                    City = "Fromage",
                    State = "WI",
                    PostalCode = "12345"
                }, _configuration["Initializer:Password"]).GetAwaiter().GetResult();

                // Retrieve newly created user from DB and give them Admin role
                ApplicationUser user = _db.ApplicationUsers.FirstOrDefault(u => u.Email == _configuration["Initializer:Email"]);
                _userManager.AddToRoleAsync(user, SD.Role_Admin).GetAwaiter().GetResult();
            }
            return;
        }
    }
}
