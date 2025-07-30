using Microsoft.AspNetCore.Mvc;
using OficinaMVC.Models;
using OficinaMVC.Services;

namespace OficinaMVC.Controllers
{
    /// <summary>
    /// Controller for handling the main site pages such as home, privacy, and error.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly IHomeService _homeService;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class.
        /// </summary>
        /// <param name="homeService">Service for home page business logic.</param>
        public HomeController(IHomeService homeService)
        {
            _homeService = homeService;
        }

        /// <summary>
        /// Displays the home page with its view model.
        /// </summary>
        /// <returns>The home page view.</returns>
        // GET: Home/Index
        public async Task<IActionResult> Index()
        {
            var viewModel = await _homeService.GetHomeViewModelAsync();
            return View(viewModel);
        }

        /// <summary>
        /// Displays the privacy policy page.
        /// </summary>
        /// <returns>The privacy policy view.</returns>
        // GET: Home/Privacy
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Displays the Terms policy page.
        /// </summary>
        /// <returns>The Terms policy view</returns>
        public IActionResult Terms()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        /// <summary>
        /// Displays the error page with the current request ID.
        /// </summary>
        /// <returns>The error view with error details.</returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        // GET: Home/Error
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}