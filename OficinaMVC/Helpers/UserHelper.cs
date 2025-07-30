using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OficinaMVC.Data.Entities;
using OficinaMVC.Models.Accounts;
using System.Security.Claims;

namespace OficinaMVC.Helpers
{
    /// <summary>
    /// Provides methods for user management, authentication, and role operations.
    /// </summary>
    public class UserHelper : IUserHelper
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserHelper"/> class.
        /// </summary>
        /// <param name="userManager">The user manager for handling user operations.</param>
        /// <param name="signInManager">The sign-in manager for authentication operations.</param>
        /// <param name="roleManager">The role manager for handling role operations.</param>
        public UserHelper(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        /// <inheritdoc />
        public async Task<IList<string>> GetRolesAsync(User user)
        {
            return await _userManager.GetRolesAsync(user);
        }

        /// <inheritdoc />
        public async Task SignInWithClaimsAsync(User user, bool isPersistent, IEnumerable<Claim> claims)
        {
            await _signInManager.SignInWithClaimsAsync(user, isPersistent, claims);
        }

        /// <inheritdoc />
        public async Task<IdentityResult> AddUserAsync(User user, string password)
        {
            return await _userManager.CreateAsync(user, password);
        }

        /// <inheritdoc />
        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        /// <inheritdoc />
        public async Task<User> GetUserByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        /// <inheritdoc />
        public async Task<SignInResult> LoginAsync(LoginViewModel model)
        {
            return await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, false);
        }

        /// <inheritdoc />
        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        /// <inheritdoc />
        public async Task<IdentityResult> UpdateUserAsync(User user)
        {
            return await _userManager.UpdateAsync(user);
        }

        /// <inheritdoc />
        public async Task<IdentityResult> ChangePasswordAsync(User user, string oldPassword, string newPassword)
        {
            return await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
        }

        /// <inheritdoc />
        public async Task CheckRoleAsync(string roleName)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        /// <inheritdoc />
        public async Task AddUserToRoleAsync(User user, string roleName)
        {
            await _userManager.AddToRoleAsync(user, roleName);
        }

        /// <inheritdoc />
        public async Task<bool> IsUserInRoleAsync(User user, string roleName)
        {
            return await _userManager.IsInRoleAsync(user, roleName);
        }

        /// <inheritdoc />
        public async Task<SignInResult> ValidatePasswordAsync(User user, string password)
        {
            return await _signInManager.CheckPasswordSignInAsync(user, password, false);
        }

        /// <inheritdoc />
        public async Task<string> GenerateEmailConfirmationTokenAsync(User user)
        {
            return await _userManager.GenerateEmailConfirmationTokenAsync(user);
        }

        /// <inheritdoc />
        public async Task<IdentityResult> ConfirmEmailAsync(User user, string token)
        {
            return await _userManager.ConfirmEmailAsync(user, token);
        }

        /// <inheritdoc />
        public async Task<string> GeneratePasswordResetTokenAsync(User user)
        {
            return await _userManager.GeneratePasswordResetTokenAsync(user);
        }

        /// <inheritdoc />
        public async Task<IdentityResult> ResetPasswordAsync(User user, string token, string password)
        {
            return await _userManager.ResetPasswordAsync(user, token, password);
        }

        /// <inheritdoc />
        public async Task<User> GetUserByNifAsync(string nif)
        {
            return await _userManager.Users.FirstOrDefaultAsync(u => u.NIF == nif);
        }

        /// <inheritdoc />
        public async Task<List<User>> GetUsersInRoleAsync(string roleName)
        {
            var users = await _userManager.GetUsersInRoleAsync(roleName);
            return users.ToList();
        }

        /// <inheritdoc />
        public async Task<IdentityResult> DeactivateUserAsync(User user)
        {
            return await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
        }

        /// <inheritdoc />
        public async Task<IdentityResult> ReactivateUserAsync(User user)
        {
            return await _userManager.SetLockoutEndDateAsync(user, null);
        }
    }
}
