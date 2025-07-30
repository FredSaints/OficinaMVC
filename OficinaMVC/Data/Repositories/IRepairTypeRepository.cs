using OficinaMVC.Data.Entities;

namespace OficinaMVC.Data.Repositories
{
    /// <summary>
    /// Repository interface for managing RepairType entities.
    /// </summary>
    public interface IRepairTypeRepository : IGenericRepository<RepairType>
    {
        /// <summary>
        /// Checks if a RepairType exists by its name.
        /// </summary>
        /// <param name="name">The name of the repair type.</param>
        /// <returns>True if exists, otherwise false.</returns>
        Task<bool> ExistsByNameAsync(string name);

        /// <summary>
        /// Checks if a RepairType exists for editing, excluding the specified id.
        /// </summary>
        /// <param name="id">The id to exclude from the check.</param>
        /// <param name="name">The name of the repair type.</param>
        /// <returns>True if exists, otherwise false.</returns>
        Task<bool> ExistsForEditAsync(int id, string name);

        /// <summary>
        /// Checks if a RepairType is currently in use.
        /// </summary>
        /// <param name="typeName">The name of the repair type.</param>
        /// <returns>True if in use, otherwise false.</returns>
        Task<bool> IsInUseAsync(string typeName);
    }
}