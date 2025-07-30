using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace OficinaMVC.Services
{
    /// <summary>
    /// Service for rendering Razor views to string for use in emails or PDFs.
    /// </summary>
    public class ViewRendererService : IViewRendererService
    {
        private readonly IRazorViewEngine _razorViewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IActionContextAccessor _actionContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewRendererService"/> class.
        /// </summary>
        /// <param name="razorViewEngine">The Razor view engine.</param>
        /// <param name="tempDataProvider">The TempData provider.</param>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        /// <param name="actionContextAccessor">The action context accessor.</param>
        public ViewRendererService(
            IRazorViewEngine razorViewEngine,
            ITempDataProvider tempDataProvider,
            IServiceProvider serviceProvider,
            IHttpContextAccessor httpContextAccessor,
            IActionContextAccessor actionContextAccessor)
        {
            _razorViewEngine = razorViewEngine;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;
            _httpContextAccessor = httpContextAccessor;
            _actionContextAccessor = actionContextAccessor;
        }

        /// <inheritdoc />
        public async Task<string> RenderToStringAsync(string viewName, object model, ViewDataDictionary viewData = null)
        {
            var actionContext = _actionContextAccessor.ActionContext ??
                                new ActionContext(_httpContextAccessor.HttpContext, _httpContextAccessor.HttpContext.GetRouteData(), new ActionDescriptor());

            using (var sw = new StringWriter())
            {
                var viewResult = _razorViewEngine.GetView(executingFilePath: null, viewPath: viewName, isMainPage: false);

                if (viewResult.View == null)
                {
                    throw new ArgumentNullException($"{viewName} does not match any available view");
                }

                var viewDictionary = viewData ?? new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary());
                viewDictionary.Model = model;

                var viewContext = new ViewContext(
                    actionContext,
                    viewResult.View,
                    viewDictionary,
                    new TempDataDictionary(actionContext.HttpContext, _tempDataProvider),
                    sw,
                    new HtmlHelperOptions()
                );

                await viewResult.View.RenderAsync(viewContext);
                return sw.ToString();
            }
        }
    }
}