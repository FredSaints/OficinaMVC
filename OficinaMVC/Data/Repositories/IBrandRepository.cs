using Microsoft.AspNetCore.Mvc.Rendering;
using OficinaMVC.Data.Entities;

namespace OficinaMVC.Data.Repositories
{
    /// <summary>
    /// Defines the contract for the brand repository, extending the generic repository
    /// with brand-specific data access methods.
    /// </summary>
    public interface IBrandRepository : IGenericRepository<Brand>
    {
        /// <summary>
        /// Gets a collection of all brands formatted as <see cref="SelectListItem"/> for use in dropdown lists.
        /// </summary>
        /// <returns>A collection of <see cref="SelectListItem"/>.</returns>
        Task<IEnumerable<SelectListItem>> GetCombo();

        /// <summary>
        /// Gets a single brand by its ID, including its collection of associated <see cref="CarModel"/> entities.
        /// </summary>
        /// <param name="id">The ID of the brand to retrieve.</param>
        /// <returns>The <see cref="Brand"/> with its models, or null if not found.</returns>
        Task<Brand> GetByIdWithModelsAsync(int id);

        /// <summary>
        /// Checks if a brand with the specified name already exists.
        /// </summary>
        /// <param name="name">The name of the brand to check.</param>
        /// <returns>True if a brand with the name exists; otherwise, false.</returns>
        Task<bool> ExistsByNameAsync(string name);

        /// <summary>
        /// Checks if a brand with the specified name already exists, excluding the brand with the given ID.
        /// This is used for validation during an edit operation.
        /// </summary>
        /// <param name="id">The ID of the brand being edited.</param>
        /// <param name="name">The name to check for duplicates.</param>
        /// <returns>True if a different brand with the same name exists; otherwise, false.</returns>
        Task<bool> ExistsForEditAsync(int id, string name);
    }
}