using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OficinaMVC.Data.Entities;

namespace OficinaMVC.Data.Repositories
{
    public class CarModelRepository : GenericRepository<CarModel>, ICarModelRepository
    {
        private readonly DataContext _context;

        public CarModelRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CarModel>> GetAllWithBrandAsync()
        {
            return await _context.CarModels.Include(cm => cm.Brand).OrderBy(cm => cm.Name).ToListAsync();
        }

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
    }
}