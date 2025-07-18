using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OficinaMVC.Data.Entities;

namespace OficinaMVC.Data.Repositories
{
    public class BrandRepository : GenericRepository<Brand>, IBrandRepository
    {
        private readonly DataContext _context;

        public BrandRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SelectListItem>> GetCombo()
        {
            var list = await _context.Brands.Select(b => new SelectListItem
            {
                Text = b.Name,
                Value = b.Id.ToString()
            }).OrderBy(b => b.Text).ToListAsync();

            list.Insert(0, new SelectListItem
            {
                Text = "Select a brand...",
                Value = "0"
            });

            return list;
        }

        public async Task<Brand> GetByIdWithModelsAsync(int id)
        {
            return await _context.Brands
                .Include(b => b.CarModels)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _context.Brands.AnyAsync(b => b.Name == name);
        }

        public async Task<bool> ExistsForEditAsync(int id, string name)
        {
            return await _context.Brands.AnyAsync(b => b.Name == name && b.Id != id);
        }
    }
}