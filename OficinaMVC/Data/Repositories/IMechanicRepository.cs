using OficinaMVC.Data.Entities;
using OficinaMVC.Models.Mechanics;

namespace OficinaMVC.Data.Repositories
{
    /// <summary>
    /// Defines the contract for the mechanic repository, handling data operations
    /// specific to users with the 'Mechanic' role.
    /// </summary>
    public interface IMechanicRepository
    {
        /// <summary>
        /// Asynchronously gets a single mechanic by their ID, including detailed related entities
        /// like their assigned specialties and work schedules.
        /// </summary>
        /// <param name="mechanicId">The unique identifier of the mechanic (User ID).</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains the
        /// detailed <see cref="User"/> object for the mechanic if found; otherwise, null.
        /// </returns>
        Task<User> GetByIdWithDetailsAsync(string mechanicId);
        /// <summary>
        /// Asynchronously updates a mechanic's specialties and work schedules based on the provided view model.
        /// This operation typically involves removing old associations and creating new ones.
        /// </summary>
        /// <param name="model">The view model containing the mechanic's ID and their new set of specialties and schedules.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result is a tuple containing:
        /// - <c>Success</c> (bool): True if the update was successful, otherwise false.
        /// - <c>ErrorMessage</c> (string): A message describing the error if the update failed, otherwise null.
        /// </returns>
        Task<(bool Success, string ErrorMessage)> UpdateMechanicAsync(MechanicEditViewModel model);
    }
}
