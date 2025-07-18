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

        public async Task<IEnumerable<Vehicle>> GetFilteredVehiclesAsync(string userId, bool isClient, string searchString)
        {
            var query = _context.Vehicles
                .Include(v => v.Owner)
                .Include(v => v.CarModel)
                .ThenInclude(cm => cm.Brand)
                .AsQueryable();

            if (isClient)
            {
                query = query.Where(v => v.OwnerId == userId);
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(v => v.LicensePlate.Contains(searchString));
            }

            return await query.OrderBy(v => v.LicensePlate).ToListAsync();
        }

        public async Task<bool> ExistsByLicensePlateAsync(string licensePlate)
        {
            return await _context.Vehicles.AnyAsync(v => v.LicensePlate == licensePlate);
        }

        public async Task<bool> ExistsByLicensePlateForEditAsync(int id, string licensePlate)
        {
            return await _context.Vehicles.AnyAsync(v => v.LicensePlate == licensePlate && v.Id != id);
        }

        public async Task<bool> IsInUseAsync(int id)
        {
            var hasAppointments = await _context.Appointments.AnyAsync(a => a.VehicleId == id);
            var hasRepairs = await _context.Repairs.AnyAsync(r => r.VehicleId == id);
            return hasAppointments || hasRepairs;
        }
    }
}