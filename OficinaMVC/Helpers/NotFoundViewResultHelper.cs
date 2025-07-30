using Microsoft.AspNetCore.Mvc;

namespace OficinaMVC.Helpers
{
    /// <summary>
    /// Provides a helper for returning NotFound view results.
    /// </summary>
    public class NotFoundViewResultHelper : INotFoundViewResultHelper
    {
        /// <inheritdoc />
        public IActionResult NotFoundView(string viewName)
        {
            return new ViewResult
            {
                ViewName = viewName
            };
        }
    }
}
