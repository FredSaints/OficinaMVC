﻿using Microsoft.AspNetCore.Identity;
using OficinaMVC.Data.Entities;
using OficinaMVC.Models;
using System.Security.Claims;

namespace OficinaMVC.Helpers
{
    public interface IUserHelper
    {
        Task<User> GetUserByNifAsync(string nif);
        Task<User> GetUserByEmailAsync(string email);
        Task<User> GetUserByIdAsync(string userId);
        Task<IdentityResult> AddUserAsync(User user, string password);
        Task<SignInResult> LoginAsync(LoginViewModel model);
        Task LogoutAsync();
        Task<IdentityResult> UpdateUserAsync(User user);
        Task<IdentityResult> ChangePasswordAsync(User user, string oldPassword, string newPassword);
        Task CheckRoleAsync(string roleName);
        Task AddUserToRoleAsync(User user, string roleName);
        Task<bool> IsUserInRoleAsync(User user, string roleName);
        Task<SignInResult> ValidatePasswordAsync(User user, string password);
        Task<string> GenerateEmailConfirmationTokenAsync(User user);
        Task<IdentityResult> ConfirmEmailAsync(User user, string token);
        Task<string> GeneratePasswordResetTokenAsync(User user);
        Task<IdentityResult> ResetPasswordAsync(User user, string token, string password);
        Task<List<User>> GetUsersInRoleAsync(string roleName);
        Task<IList<string>> GetRolesAsync(User user);
        Task SignInWithClaimsAsync(User user, bool isPersistent, IEnumerable<Claim> claims);
    }
}
