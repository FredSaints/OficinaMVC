using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OficinaMVC.Controllers
{
    /// <summary>
    /// Controller for admin-related actions. Accessible only by users in the Admin role.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        /// <summary>
        /// Displays the admin dashboard.
        /// </summary>
        /// <returns>The admin dashboard view.</returns>
        // GET: Admin/Index
        public IActionResult Index()
        {
            return View();
        }
    }
}