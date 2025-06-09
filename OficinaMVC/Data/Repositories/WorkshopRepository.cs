using Microsoft.EntityFrameworkCore;
using OficinaMVC.Data.Entities;

namespace OficinaMVC.Data.Repositories
{
    public class WorkshopRepository : IWorkshopRepository
    {
        private readonly DataContext _context;

        public WorkshopRepository(DataContext context)
        {
            _context = context;
        }

        // ---------------- VEHICLES ----------------
        public void AddVehicle(Vehicle vehicle) => _context.Vehicles.Add(vehicle);

        public void UpdateVehicle(Vehicle vehicle) => _context.Vehicles.Update(vehicle);

        public void RemoveVehicle(Vehicle vehicle) => _context.Vehicles.Remove(vehicle);

        public IEnumerable<Vehicle> GetVehiclesByUser(string userId) =>
            _context.Vehicles
                .Where(v => v.OwnerId == userId)
                .OrderBy(v => v.LicensePlate);

        public async Task<Vehicle?> GetVehicleByIdAsync(int id) =>
            await _context.Vehicles
                .Include(v => v.Repairs)
                .FirstOrDefaultAsync(v => v.Id == id);

        public bool VehicleExists(int id) =>
            _context.Vehicles.Any(v => v.Id == id);

        // ---------------- APPOINTMENTS ----------------
        public void AddAppointment(Appointment appointment) => _context.Appointments.Add(appointment);

        public void UpdateAppointment(Appointment appointment) => _context.Appointments.Update(appointment);

        public void RemoveAppointment(Appointment appointment) => _context.Appointments.Remove(appointment);

        public async Task<Appointment?> GetAppointmentByIdAsync(int id) =>
            await _context.Appointments
                .Include(a => a.Mechanic)
                .Include(a => a.Vehicle)
                .FirstOrDefaultAsync(a => a.Id == id);

        public IEnumerable<Appointment> GetAppointmentsByUser(string userId) =>
            _context.Appointments
                .Where(a => a.ClientId == userId)
                .OrderByDescending(a => a.Date);

        // ---------------- REPAIRS ----------------
        public void AddRepair(Repair repair) => _context.Repairs.Add(repair);

        public void UpdateRepair(Repair repair) => _context.Repairs.Update(repair);

        public void RemoveRepair(Repair repair) => _context.Repairs.Remove(repair);

        public async Task<Repair?> GetRepairByIdAsync(int id) =>
            await _context.Repairs
                .Include(r => r.Vehicle)
                .Include(r => r.Mechanics)
                .FirstOrDefaultAsync(r => r.Id == id);

        public IEnumerable<Repair> GetRepairsByVehicle(int vehicleId) =>
            _context.Repairs
                .Where(r => r.VehicleId == vehicleId)
                .OrderByDescending(r => r.StartDate);

        // ---------------- SAVE ----------------
        public async Task<bool> SaveAllAsync() =>
            await _context.SaveChangesAsync() > 0;
    }
}