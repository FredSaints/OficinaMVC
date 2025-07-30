using Microsoft.AspNetCore.Identity;
using OficinaMVC.Data.Entities;

namespace OficinaMVC.Data
{
    /// <summary>
    /// Provides methods to seed the database with initial roles and a default admin user.
    /// </summary>
    public class SeedDb
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="SeedDb"/> class.
        /// </summary>
        /// <param name="userManager">The user manager for handling user operations.</param>
        /// <param name="roleManager">The role manager for handling role operations.</param>
        public SeedDb(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        /// <summary>
        /// Seeds the database with default roles and a default admin user if they do not exist.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task SeedAsync()
        {
            // 1. Ensure roles
            await CheckRoleAsync("Admin");
            await CheckRoleAsync("Receptionist");
            await CheckRoleAsync("Mechanic");
            await CheckRoleAsync("Client");
           // await CheckRoleAsync("Anonymous");

            // 2. Ensure default admin user
            var adminEmail = "admin@oficina.com";
            var adminUser = await _userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var user = new User
                {
                    FirstName = "System",
                    LastName = "Administrator",
                    Email = adminEmail,
                    UserName = adminEmail,
                    NIF = "123456789",
                    PhoneNumber = "910000000",
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, "123456");

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Admin");
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new Exception($"Failed to create admin user: {errors}");
                }
            }
        }

        /// <summary>
        /// Checks if a role exists, and creates it if it does not.
        /// </summary>
        /// <param name="roleName">The name of the role to check or create.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task CheckRoleAsync(string roleName)
        {
            var exists = await _roleManager.RoleExistsAsync(roleName);
            if (!exists)
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }
}
