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
                .Include(v => v.Owner)
                .Include(v => v.CarModel)
                    .ThenInclude(cm => cm.Brand)
                .Where(v => v.OwnerId == ownerId)
                .ToListAsync();
        }

        public async Task<List<Vehicle>> GetAllWithDetailsAsync()
        {
            return await _context.Vehicles
                .Include(v => v.Owner)
                .Include(v => v.CarModel)
                    .ThenInclude(cm => cm.Brand)
                .ToListAsync();
        }

        public async Task<Vehicle> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Vehicles
                .Include(v => v.Owner)
                .Include(v => v.CarModel)
                    .ThenInclude(cm => cm.Brand)
                .FirstOrDefaultAsync(v => v.Id == id);
        }
    }
}