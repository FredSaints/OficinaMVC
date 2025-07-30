using OficinaMVC.Data.Entities;

namespace OficinaMVC.Services
{
    /// <summary>
    /// Provides methods for retrieving user information from the database.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Safely retrieves the currently authenticated user from the database.
        /// </summary>
        /// <returns>The User entity if found and authenticated; otherwise, null.</returns>
        Task<User?> GetCurrentUserAsync();
    }
}