using Microsoft.AspNetCore.Identity;
using OficinaMVC.Data.Entities;
using OficinaMVC.Models.Accounts;
using System.Security.Claims;

namespace OficinaMVC.Helpers
{
    /// <summary>
    /// Defines methods for user management, authentication, and role operations.
    /// </summary>
    public interface IUserHelper
    {
        /// <summary>
        /// Gets a user by their NIF (tax identification number).
        /// </summary>
        /// <param name="nif">The NIF of the user.</param>
        /// <returns>The user with the specified NIF, or null if not found.</returns>
        Task<User> GetUserByNifAsync(string nif);

        /// <summary>
        /// Gets a user by their email address.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        /// <returns>The user with the specified email, or null if not found.</returns>
        Task<User> GetUserByEmailAsync(string email);

        /// <summary>
        /// Gets a user by their unique identifier.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>The user with the specified ID, or null if not found.</returns>
        Task<User> GetUserByIdAsync(string userId);

        /// <summary>
        /// Adds a new user with the specified password.
        /// </summary>
        /// <param name="user">The user entity to add.</param>
        /// <param name="password">The password for the new user.</param>
        /// <returns>The result of the user creation operation.</returns>
        Task<IdentityResult> AddUserAsync(User user, string password);

        /// <summary>
        /// Logs in a user using the provided login view model.
        /// </summary>
        /// <param name="model">The login view model containing credentials.</param>
        /// <returns>The result of the sign-in attempt.</returns>
        Task<SignInResult> LoginAsync(LoginViewModel model);

        /// <summary>
        /// Logs out the currently authenticated user.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task LogoutAsync();

        /// <summary>
        /// Updates the specified user entity.
        /// </summary>
        /// <param name="user">The user entity to update.</param>
        /// <returns>The result of the update operation.</returns>
        Task<IdentityResult> UpdateUserAsync(User user);

        /// <summary>
        /// Changes the password for the specified user.
        /// </summary>
        /// <param name="user">The user entity.</param>
        /// <param name="oldPassword">The current password.</param>
        /// <param name="newPassword">The new password.</param>
        /// <returns>The result of the password change operation.</returns>
        Task<IdentityResult> ChangePasswordAsync(User user, string oldPassword, string newPassword);

        /// <summary>
        /// Checks if a role exists, and creates it if it does not.
        /// </summary>
        /// <param name="roleName">The name of the role to check or create.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task CheckRoleAsync(string roleName);

        /// <summary>
        /// Adds a user to the specified role.
        /// </summary>
        /// <param name="user">The user entity.</param>
        /// <param name="roleName">The name of the role.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddUserToRoleAsync(User user, string roleName);

        /// <summary>
        /// Checks if a user is in the specified role.
        /// </summary>
        /// <param name="user">The user entity.</param>
        /// <param name="roleName">The name of the role.</param>
        /// <returns>True if the user is in the role; otherwise, false.</returns>
        Task<bool> IsUserInRoleAsync(User user, string roleName);

        /// <summary>
        /// Validates the user's password.
        /// </summary>
        /// <param name="user">The user entity.</param>
        /// <param name="password">The password to validate.</param>
        /// <returns>The result of the password validation.</returns>
        Task<SignInResult> ValidatePasswordAsync(User user, string password);

        /// <summary>
        /// Generates an email confirmation token for the specified user.
        /// </summary>
        /// <param name="user">The user entity.</param>
        /// <returns>The email confirmation token.</returns>
        Task<string> GenerateEmailConfirmationTokenAsync(User user);

        /// <summary>
        /// Confirms the user's email using the provided token.
        /// </summary>
        /// <param name="user">The user entity.</param>
        /// <param name="token">The confirmation token.</param>
        /// <returns>The result of the email confirmation operation.</returns>
        Task<IdentityResult> ConfirmEmailAsync(User user, string token);

        /// <summary>
        /// Generates a password reset token for the specified user.
        /// </summary>
        /// <param name="user">The user entity.</param>
        /// <returns>The password reset token.</returns>
        Task<string> GeneratePasswordResetTokenAsync(User user);

        /// <summary>
        /// Resets the user's password using the provided token and new password.
        /// </summary>
        /// <param name="user">The user entity.</param>
        /// <param name="token">The password reset token.</param>
        /// <param name="password">The new password.</param>
        /// <returns>The result of the password reset operation.</returns>
        Task<IdentityResult> ResetPasswordAsync(User user, string token, string password);

        /// <summary>
        /// Gets a list of users in the specified role.
        /// </summary>
        /// <param name="roleName">The name of the role.</param>
        /// <returns>A list of users in the role.</returns>
        Task<List<User>> GetUsersInRoleAsync(string roleName);

        /// <summary>
        /// Gets the roles assigned to the specified user.
        /// </summary>
        /// <param name="user">The user entity.</param>
        /// <returns>A list of role names assigned to the user.</returns>
        Task<IList<string>> GetRolesAsync(User user);

        /// <summary>
        /// Signs in the user with the specified claims.
        /// </summary>
        /// <param name="user">The user entity.</param>
        /// <param name="isPersistent">Whether the sign-in session should persist across browser sessions.</param>
        /// <param name="claims">The claims to associate with the sign-in.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SignInWithClaimsAsync(User user, bool isPersistent, IEnumerable<Claim> claims);

        Task<IdentityResult> DeactivateUserAsync(User user);
        Task<IdentityResult> ReactivateUserAsync(User user);
    }
}
