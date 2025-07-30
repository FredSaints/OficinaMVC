using Microsoft.AspNetCore.Mvc.Rendering;
using OficinaMVC.Data.Entities;

namespace OficinaMVC.Data.Repositories
{
    /// <summary>
    /// Defines the contract for the car model repository, extending the generic repository
    /// with car model-specific data access methods.
    /// </summary>
    public interface ICarModelRepository : IGenericRepository<CarModel>
    {
        /// <summary>
        /// Gets a collection of all car models, including their associated <see cref="Brand"/> information.
        /// </summary>
        /// <returns>A collection of <see cref="CarModel"/> entities with their brands included.</returns>
        Task<IEnumerable<CarModel>> GetAllWithBrandAsync();

        /// <summary>
        /// Gets a collection of car models for a specific brand, formatted as <see cref="SelectListItem"/> for use in dropdown lists.
        /// </summary>
        /// <param name="brandId">The ID of the parent brand.</param>
        /// <returns>A collection of <see cref="SelectListItem"/> for the specified brand's models.</returns>
        Task<IEnumerable<SelectListItem>> GetCombo(int brandId);

        /// <summary>
        /// Checks if a car model with the specified name already exists for a given brand.
        /// </summary>
        /// <param name="name">The name of the car model to check.</param>
        /// <param name="brandId">The ID of the parent brand.</param>
        /// <returns>True if a model with the same name and brand exists; otherwise, false.</returns>
        Task<bool> ExistsByNameAndBrandAsync(string name, int brandId);

        /// <summary>
        /// Checks if a car model with the specified name already exists for a given brand, excluding the model with the given ID.
        /// This is used for validation during an edit operation.
        /// </summary>
        /// <param name="id">The ID of the car model being edited.</param>
        /// <param name="name">The name to check for duplicates.</param>
        /// <param name="brandId">The ID of the parent brand.</param>
        /// <returns>True if a different model with the same name and brand exists; otherwise, false.</returns>
        Task<bool> ExistsForEditAsync(int id, string name, int brandId);

        /// <summary>
        /// Checks if a car model is currently associated with any <see cref="Vehicle"/> entities.
        /// This is used to prevent deletion if the model is in use.
        /// </summary>
        /// <param name="id">The ID of the car model to check.</param>
        /// <returns>True if the model is in use; otherwise, false.</returns>
        Task<bool> IsInUseAsync(int id);
    }
}