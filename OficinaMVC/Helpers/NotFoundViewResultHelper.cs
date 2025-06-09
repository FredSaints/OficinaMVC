using Microsoft.AspNetCore.Mvc;

namespace OficinaMVC.Helpers
{
    public class NotFoundViewResultHelper : INotFoundViewResultHelper
    {
        public IActionResult NotFoundView(string viewName)
        {
            return new ViewResult
            {
                ViewName = viewName
            };
        }
    }
}
