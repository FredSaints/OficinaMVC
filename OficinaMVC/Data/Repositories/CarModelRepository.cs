using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OficinaMVC.Data.Entities;

namespace OficinaMVC.Data.Repositories
{
    /// <summary>
    /// Repository for handling data operations for <see cref="CarModel"/> entities.
    /// </summary>
    public class CarModelRepository : GenericRepository<CarModel>, ICarModelRepository
    {
        private readonly DataContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="CarModelRepository"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        public CarModelRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets a collection of all car models, including their associated <see cref="Brand"/> information.
        /// </summary>
        /// <returns>A collection of <see cref="CarModel"/> entities with their brands included, ordered by name.</returns>
        public async Task<IEnumerable<CarModel>> GetAllWithBrandAsync()
        {
            return await _context.CarModels.Include(cm => cm.Brand).OrderBy(cm => cm.Name).ToListAsync();
        }

        /// <summary>
        /// Gets a collection of car models for a specific brand, formatted as <see cref="SelectListItem"/> for use in dropdown lists.
        /// </summary>
        /// <param name="brandId">The ID of the parent brand.</param>
        /// <returns>A collection of <see cref="SelectListItem"/> for the specified brand's models, with a default "Select a model..." option.</returns>
        public async Task<IEnumerable<SelectListItem>> GetCombo(int brandId)
        {
            var list = await _context.CarModels
                .Where(cm => cm.BrandId == brandId)
                .Select(cm => new SelectListItem
                {
                    Text = cm.Name,
                    Value = cm.Id.ToString()
                })
                .OrderBy(cm => cm.Text)
                .ToListAsync();

            list.Insert(0, new SelectListItem
            {
                Text = "Select a model...",
                Value = "0"
            });

            return list;
        }

        /// <summary>
        /// Checks if a car model with the specified name already exists for a given brand.
        /// </summary>
        /// <param name="name">The name of the car model to check.</param>
        /// <param name="brandId">The ID of the parent brand.</param>
        /// <returns>True if a model with the same name and brand exists; otherwise, false.</returns>
        public async Task<bool> ExistsByNameAndBrandAsync(string name, int brandId)
        {
            return await _context.CarModels.AnyAsync(m => m.Name == name && m.BrandId == brandId);
        }

        /// <summary>
        /// Checks if a car model with the specified name already exists for a given brand, excluding the model with the given ID.
        /// This is used for validation during an edit operation.
        /// </summary>
        /// <param name="id">The ID of the car model being edited.</param>
        /// <param name="name">The name to check for duplicates.</param>
        /// <param name="brandId">The ID of the parent brand.</param>
        /// <returns>True if a different model with the same name and brand exists; otherwise, false.</returns>
        public async Task<bool> ExistsForEditAsync(int id, string name, int brandId)
        {
            return await _context.CarModels.AnyAsync(m => m.Name == name && m.BrandId == brandId && m.Id != id);
        }

        /// <summary>
        /// Checks if a car model is currently associated with any <see cref="Vehicle"/> entities.
        /// This is used to prevent deletion if the model is in use.
        /// </summary>
        /// <param name="id">The ID of the car model to check.</param>
        /// <returns>True if the model is in use; otherwise, false.</returns>
        public async Task<bool> IsInUseAsync(int id)
        {
            return await _context.Vehicles.AnyAsync(v => v.CarModelId == id);
        }
    }
}