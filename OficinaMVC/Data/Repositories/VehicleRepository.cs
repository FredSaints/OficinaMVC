using Microsoft.EntityFrameworkCore;
using OficinaMVC.Data.Entities;

namespace OficinaMVC.Data.Repositories
{
    public class VehicleRepository : GenericRepository<Vehicle>, IVehicleRepository
    {
        private readonly DataContext _context;

        public VehicleRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Vehicle>> GetVehiclesByOwnerIdAsync(string ownerId)
        {
            return await _context.Vehicles
                .AsNoTracking()
                .Where(v => v.OwnerId == ownerId)
                .ToListAsync();
        }
    }
}
