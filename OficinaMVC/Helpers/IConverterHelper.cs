using OficinaMVC.Data.Entities;
using OficinaMVC.Models.Accounts;

namespace OficinaMVC.Helpers
{
    /// <summary>
    /// Defines methods for converting between user entities and view models.
    /// </summary>
    public interface IConverterHelper
    {
        /// <summary>
        /// Converts a registration view model to a user entity.
        /// </summary>
        /// <param name="model">The registration view model.</param>
        /// <returns>A user entity populated with data from the view model.</returns>
        Task<User> ToUserEntityAsync(RegisterViewModel model);

        /// <summary>
        /// Converts a user entity to a registration view model.
        /// </summary>
        /// <param name="user">The user entity.</param>
        /// <returns>A registration view model populated with data from the user entity.</returns>
        RegisterViewModel ToRegisterViewModel(User user);
    }
}
