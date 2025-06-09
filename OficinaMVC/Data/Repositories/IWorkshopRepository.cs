using OficinaMVC.Data.Entities;

namespace OficinaMVC.Data.Repositories
{
    public interface IWorkshopRepository
    {
        // Vehicle
        void AddVehicle(Vehicle vehicle);
        void UpdateVehicle(Vehicle vehicle);
        void RemoveVehicle(Vehicle vehicle);
        IEnumerable<Vehicle> GetVehiclesByUser(string userId);
        Task<Vehicle?> GetVehicleByIdAsync(int id);
        bool VehicleExists(int id);

        // Appointment
        void AddAppointment(Appointment appointment);
        Task<Appointment?> GetAppointmentByIdAsync(int id);
        IEnumerable<Appointment> GetAppointmentsByUser(string userId);
        void UpdateAppointment(Appointment appointment);
        void RemoveAppointment(Appointment appointment);

        // Repair
        void AddRepair(Repair repair);
        Task<Repair?> GetRepairByIdAsync(int id);
        IEnumerable<Repair> GetRepairsByVehicle(int vehicleId);
        void UpdateRepair(Repair repair);
        void RemoveRepair(Repair repair);

        // General
        Task<bool> SaveAllAsync();
    }
}