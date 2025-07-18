using Microsoft.EntityFrameworkCore;
using OficinaMVC.Data.Entities;

namespace OficinaMVC.Data.Repositories
{
    public class RepairTypeRepository : GenericRepository<RepairType>, IRepairTypeRepository
    {
        private readonly DataContext _context;

        public RepairTypeRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _context.RepairTypes.AnyAsync(rt => rt.Name == name);
        }

        public async Task<bool> ExistsForEditAsync(int id, string name)
        {
            return await _context.RepairTypes.AnyAsync(rt => rt.Name == name && rt.Id != id);
        }

        public async Task<bool> IsInUseAsync(string typeName)
        {
            return await _context.Appointments.AnyAsync(a => a.ServiceType == typeName);
        }
    }
}