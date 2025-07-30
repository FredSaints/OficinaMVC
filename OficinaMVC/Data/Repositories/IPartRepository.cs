using OficinaMVC.Data.Entities;
using System.Threading.Tasks;

namespace OficinaMVC.Data.Repositories
{
    /// <summary>
    /// Defines the contract for the part repository, extending the generic repository
    /// with part-specific data access methods.
    /// </summary>
    public interface IPartRepository : IGenericRepository<Part>
    {
        /// <summary>
        /// Asynchronously checks if a part with the specified name already exists.
        /// </summary>
        /// <param name="name">The name of the part to check.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if a part with the name exists; otherwise, false.</returns>
        Task<bool> ExistsByNameAsync(string name);

        /// <summary>
        /// Asynchronously checks if a part with the specified name already exists, excluding the part with the given ID.
        /// This is used for validation during an edit operation.
        /// </summary>
        /// <param name="id">The ID of the part being edited.</param>
        /// <param name="name">The name to check for duplicates.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if a different part with the same name exists; otherwise, false.</returns>
        Task<bool> ExistsForEditAsync(int id, string name);

        /// <summary>
        /// Asynchronously checks if a part is currently associated with any <see cref="RepairPart"/> entities.
        /// This is used to prevent deletion if the part has been used in a repair.
        /// </summary>
        /// <param name="id">The ID of the part to check.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the part is in use; otherwise, false.</returns>
        Task<bool> IsInUseAsync(int id);
    }
}