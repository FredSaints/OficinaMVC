using OficinaMVC.Models.Dashboard;
using System.Security.Claims;

namespace OficinaMVC.Services
{
    /// <summary>
    /// Provides methods for retrieving dashboard data for the application.
    /// </summary>
    public interface IDashboardService
    {
        /// <summary>
        /// Gets the dashboard view model for the specified user.
        /// </summary>
        /// <param name="userPrincipal">The user principal for which to retrieve dashboard data.</param>
        /// <returns>The dashboard view model.</returns>
        Task<DashboardViewModel> GetDashboardViewModelAsync(ClaimsPrincipal userPrincipal);
    }
}