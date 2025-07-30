using OficinaMVC.Data.Entities;
using OficinaMVC.Helpers;

namespace OficinaMVC.Services
{
    /// <summary>
    /// Service for retrieving user information from the database and the current HTTP context.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserHelper _userHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserService"/> class.
        /// </summary>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        /// <param name="userHelper">The user helper for database operations.</param>
        public UserService(IHttpContextAccessor httpContextAccessor, IUserHelper userHelper)
        {
            _httpContextAccessor = httpContextAccessor;
            _userHelper = userHelper;
        }

        /// <inheritdoc />
        public async Task<User?> GetCurrentUserAsync()
        {
            var userPrincipal = _httpContextAccessor.HttpContext?.User;
            if (userPrincipal == null || !userPrincipal.Identity.IsAuthenticated)
            {
                return null;
            }

            var userEmail = userPrincipal.Identity.Name;
            if (string.IsNullOrEmpty(userEmail))
            {
                return null;
            }

            return await _userHelper.GetUserByEmailAsync(userEmail);
        }
    }
}