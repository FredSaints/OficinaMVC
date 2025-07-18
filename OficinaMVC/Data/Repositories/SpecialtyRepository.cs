using Microsoft.EntityFrameworkCore;
using OficinaMVC.Data.Entities;

namespace OficinaMVC.Data.Repositories
{
    public class SpecialtyRepository : GenericRepository<Specialty>, ISpecialtyRepository
    {
        private readonly DataContext _context;

        public SpecialtyRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public override async Task<IEnumerable<Specialty>> GetAllAsync()
        {
            return await _context.Specialties.AsNoTracking().ToListAsync();
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _context.Specialties.AnyAsync(s => s.Name == name);
        }

        public async Task<bool> ExistsForEditAsync(int id, string name)
        {
            return await _context.Specialties.AnyAsync(s => s.Name == name && s.Id != id);
        }

        public async Task<bool> IsInUseAsync(int id)
        {
            return await _context.UserSpecialties.AnyAsync(us => us.SpecialtyId == id);
        }
    }
}