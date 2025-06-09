using Microsoft.AspNetCore.Mvc;

namespace OficinaMVC.Helpers
{
    public interface INotFoundViewResultHelper
    {
        IActionResult NotFoundView(string viewName);
    }
}
