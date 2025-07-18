using Microsoft.EntityFrameworkCore;
using OficinaMVC.Data.Entities;

namespace OficinaMVC.Data.Repositories
{
    public class PartRepository : GenericRepository<Part>, IPartRepository
    {
        private readonly DataContext _context;

        public PartRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _context.Parts.AnyAsync(p => p.Name == name);
        }

        public async Task<bool> ExistsForEditAsync(int id, string name)
        {
            return await _context.Parts.AnyAsync(p => p.Name == name && p.Id != id);
        }

        public async Task<bool> IsInUseAsync(int id)
        {
            return await _context.RepairParts.AnyAsync(rp => rp.PartId == id);
        }
    }
}