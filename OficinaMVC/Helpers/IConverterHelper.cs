using OficinaMVC.Data.Entities;
using OficinaMVC.Models;

namespace OficinaMVC.Helpers
{
    public interface IConverterHelper
    {
        Task<User> ToUserEntityAsync(RegisterViewModel model);
        RegisterViewModel ToRegisterViewModel(User user);
    }
}
