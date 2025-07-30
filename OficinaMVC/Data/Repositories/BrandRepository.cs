using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OficinaMVC.Data.Entities;

namespace OficinaMVC.Data.Repositories
{
    /// <summary>
    /// Repository for handling data operations for <see cref="Brand"/> entities.
    /// </summary>
    public class BrandRepository : GenericRepository<Brand>, IBrandRepository
    {
        private readonly DataContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="BrandRepository"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        public BrandRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets a collection of all brands formatted as <see cref="SelectListItem"/> for use in dropdown lists.
        /// </summary>
        /// <returns>A collection of <see cref="SelectListItem"/> with a default "Select a brand..." option.</returns>
        public async Task<IEnumerable<SelectListItem>> GetCombo()
        {
            var list = await _context.Brands.Select(b => new SelectListItem
            {
                Text = b.Name,
                Value = b.Id.ToString()
            }).OrderBy(b => b.Text).ToListAsync();

            return list;
        }

        /// <summary>
        /// Gets a single brand by its ID, including its collection of associated <see cref="CarModel"/> entities.
        /// </summary>
        /// <param name="id">The ID of the brand to retrieve.</param>
        /// <returns>The <see cref="Brand"/> with its models, or null if not found.</returns>
        public async Task<Brand> GetByIdWithModelsAsync(int id)
        {
            return await _context.Brands
                .Include(b => b.CarModels)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        /// <summary>
        /// Checks if a brand with the specified name already exists.
        /// </summary>
        /// <param name="name">The name of the brand to check.</param>
        /// <returns>True if a brand with the name exists; otherwise, false.</returns>
        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _context.Brands.AnyAsync(b => b.Name == name);
        }

        /// <summary>
        /// Checks if a brand with the specified name already exists, excluding the brand with the given ID.
        /// This is used for validation during an edit operation.
        /// </summary>
        /// <param name="id">The ID of the brand being edited.</param>
        /// <param name="name">The name to check for duplicates.</param>
        /// <returns>True if a different brand with the same name exists; otherwise, false.</returns>
        public async Task<bool> ExistsForEditAsync(int id, string name)
        {
            return await _context.Brands.AnyAsync(b => b.Name == name && b.Id != id);
        }
    }
}