using Microsoft.AspNetCore.Mvc.Rendering;
using OficinaMVC.Data.Entities;

namespace OficinaMVC.Data.Repositories
{
    public interface IBrandRepository : IGenericRepository<Brand>
    {
        /// <summary>
        /// Gets a collection of brands formatted for use in a dropdown list.
        /// </summary>
        /// <returns>An IEnumerable of SelectListItem.</returns>
        Task<IEnumerable<SelectListItem>> GetCombo();

        /// <summary>
        /// Gets a brand by its ID, including its collection of associated CarModels.
        /// </summary>
        /// <param name="brandId">The ID of the brand.</param>
        /// <returns>The Brand entity with its CarModels, or null if not found.</returns>
        Task<Brand> GetByIdWithModelsAsync(int brandId);
    }
}