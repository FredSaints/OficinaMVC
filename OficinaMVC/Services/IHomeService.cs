using OficinaMVC.Models.Home;

namespace OficinaMVC.Services
{
    public interface IHomeService
    {
        Task<HomeViewModel> GetHomeViewModelAsync();
    }
}