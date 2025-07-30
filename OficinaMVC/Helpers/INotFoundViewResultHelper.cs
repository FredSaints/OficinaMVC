using Microsoft.AspNetCore.Mvc;

namespace OficinaMVC.Helpers
{
    /// <summary>
    /// Defines a helper for returning a NotFound view result.
    /// </summary>
    public interface INotFoundViewResultHelper
    {
        /// <summary>
        /// Returns a NotFound view result for the specified view name.
        /// </summary>
        /// <param name="viewName">The name of the view to return.</param>
        /// <returns>An <see cref="IActionResult"/> representing the NotFound view.</returns>
        IActionResult NotFoundView(string viewName);
    }
}
