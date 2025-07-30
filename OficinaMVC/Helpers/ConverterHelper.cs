using OficinaMVC.Data.Entities;
using OficinaMVC.Models.Accounts;

namespace OficinaMVC.Helpers
{
    /// <summary>
    /// Provides methods to convert between user entities and view models.
    /// </summary>
    public class ConverterHelper : IConverterHelper
    {
        /// <summary>
        /// Converts a <see cref="RegisterViewModel"/> to a <see cref="User"/> entity.
        /// </summary>
        /// <param name="model">The registration view model.</param>
        /// <returns>A user entity populated with data from the view model.</returns>
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

        /// <summary>
        /// Converts a <see cref="User"/> entity to a <see cref="RegisterViewModel"/>.
        /// </summary>
        /// <param name="user">The user entity.</param>
        /// <returns>A registration view model populated with data from the user entity.</returns>
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
