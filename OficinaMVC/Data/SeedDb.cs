using Microsoft.AspNetCore.Identity;
using OficinaMVC.Data.Entities;

namespace OficinaMVC.Data
{
    public class SeedDb
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public SeedDb(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

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
