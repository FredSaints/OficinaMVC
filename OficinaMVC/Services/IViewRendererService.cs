using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace OficinaMVC.Services
{
    /// <summary>
    /// Provides methods for rendering Razor views to string for use in emails or PDFs.
    /// </summary>
    public interface IViewRendererService
    {
        /// <summary>
        /// Renders a Razor view to a string using the specified model and optional view data.
        /// </summary>
        /// <param name="viewName">The name or path of the view to render.</param>
        /// <param name="model">The model to pass to the view.</param>
        /// <param name="viewData">Optional view data for the view.</param>
        /// <returns>The rendered view as a string.</returns>
        Task<string> RenderToStringAsync(string viewName, object model, ViewDataDictionary viewData = null);
    }
}