using OficinaMVC.Data.Entities;

namespace OficinaMVC.Data.Repositories
{
    /// <summary>
    /// Defines repository operations specific to the Specialty entity.
    /// </summary>
    public interface ISpecialtyRepository : IGenericRepository<Specialty>
    {
        /// <summary>
        /// Checks if a specialty with the given name exists.
        /// </summary>
        /// <param name="name">The name of the specialty.</param>
        /// <returns>True if a specialty with the given name exists; otherwise, false.</returns>
        Task<bool> ExistsByNameAsync(string name);
        /// <summary>
        /// Checks if a specialty with the given name exists for editing, excluding the specified ID.
        /// </summary>
        /// <param name="id">The ID to exclude from the check.</param>
        /// <param name="name">The name of the specialty.</param>
        /// <returns>True if a specialty with the given name exists for another ID; otherwise, false.</returns>
        Task<bool> ExistsForEditAsync(int id, string name);
        /// <summary>
        /// Checks if the specialty is currently in use.
        /// </summary>
        /// <param name="id">The ID of the specialty.</param>
        /// <returns>True if the specialty is in use; otherwise, false.</returns>
        Task<bool> IsInUseAsync(int id);
    }
}
