using OficinaMVC.Models.Dashboard;
using System.Security.Claims;

namespace OficinaMVC.Services
{
    public interface IDashboardService
    {
        Task<DashboardViewModel> GetDashboardViewModelAsync(ClaimsPrincipal userPrincipal);
    }
}