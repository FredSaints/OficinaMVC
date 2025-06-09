using OficinaMVC.Data.Entities;
using OficinaMVC.Models;

namespace OficinaMVC.Helpers
{
    public class ConverterHelper : IConverterHelper
    {
        public async Task<User> ToUserEntityAsync(RegisterViewModel model)
        {
            return new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                NIF = model.NIF,
                UserName = model.Email
            };
        }

        public RegisterViewModel ToRegisterViewModel(User user)
        {
            return new RegisterViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                NIF = user.NIF
            };
        }
    }
}
