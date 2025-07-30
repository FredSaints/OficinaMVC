using OficinaMVC.Models.Home;

namespace OficinaMVC.Services
{
    /// <summary>
    /// Provides methods for retrieving data for the application's home page.
    /// </summary>
    public interface IHomeService
    {
        /// <summary>
        /// Gets the home view model containing services, mechanics, and opening hours.
        /// </summary>
        /// <returns>The home view model.</returns>
        Task<HomeViewModel> GetHomeViewModelAsync();
    }
}