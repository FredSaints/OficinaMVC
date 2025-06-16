using Microsoft.EntityFrameworkCore;
using OficinaMVC.Data.Entities;

namespace OficinaMVC.Data.Repositories
{
    public class SpecialtyRepository : GenericRepository<Specialty>, ISpecialtyRepository
    {
        public SpecialtyRepository(DataContext context) : base(context) { }

        public override async Task<IEnumerable<Specialty>> GetAllAsync()
        {
            return await _context.Specialties.AsNoTracking().ToListAsync();
        }
    }
}