using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMVC.Services;

namespace OficinaMVC.Controllers
{
    /// <summary>
    /// Controller for displaying the dashboard for receptionists and mechanics.
    /// </summary>
    [Authorize(Roles = "Receptionist,Mechanic")]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DashboardController"/> class.
        /// </summary>
        /// <param name="dashboardService">Service for dashboard-related business logic.</param>
        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        /// <summary>
        /// Displays the dashboard view with relevant data for the current user.
        /// </summary>
        /// <returns>The dashboard view with its view model.</returns>
        // GET: Dashboard/Index
        public async Task<IActionResult> Index()
        {
            var viewModel = await _dashboardService.GetDashboardViewModelAsync(User);

            return View(viewModel);
        }
    }
}