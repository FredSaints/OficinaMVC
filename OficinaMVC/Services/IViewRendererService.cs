using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace OficinaMVC.Services
{
    public interface IViewRendererService
    {
        Task<string> RenderToStringAsync(string viewName, object model, ViewDataDictionary viewData = null);
    }
}