using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OficinaMVC.Models;

namespace OficinaMVC.Controllers
{
    /// <summary>
    /// Controller for handling application errors and displaying error views.
    /// </summary>
    [AllowAnonymous]
    public class ErrorController : Controller
    {
        /// <summary>
        /// Handles HTTP status code errors and displays the appropriate error view.
        /// </summary>
        /// <param name="statusCode">The HTTP status code.</param>
        /// <returns>The error view corresponding to the status code.</returns>
        [Route("Error/{statusCode}")]
        // GET: Error/{statusCode}
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            var statusCodeResult = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();

            var viewModel = new ErrorViewModel
            {
                StatusCode = statusCode,
                OriginalPath = statusCodeResult?.OriginalPath,
                RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier
            };

            switch (statusCode)
            {
                case 403:
                    viewModel.ErrorMessage = "You are not authorized to access this page.";
                    return View("NotAuthorized", viewModel);
                case 404:
                    viewModel.ErrorMessage = "The page you are looking for could not be found.";
                    return View("NotFound", viewModel);
                default:
                    viewModel.ErrorMessage = $"An error occurred with status code {statusCode}.";
                    return View("Error", viewModel);
            }
        }

        /// <summary>
        /// Handles unhandled exceptions and displays the error view.
        /// </summary>
        /// <returns>The error view with exception details.</returns>
        [Route("Error")]
        // GET: Error
        public IActionResult Error()
        {
            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            var viewModel = new ErrorViewModel
            {
                ErrorMessage = exceptionHandlerPathFeature?.Error.Message ?? "An unexpected error occurred.",
                OriginalPath = exceptionHandlerPathFeature?.Path,
                RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier
            };

            return View(viewModel);
        }
    }
}