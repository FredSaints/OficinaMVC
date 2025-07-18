using OficinaMVC.Data.Entities;

namespace OficinaMVC.Data.Repositories
{
    public interface IVehicleRepository : IGenericRepository<Vehicle>
    {
        Task<List<Vehicle>> GetVehiclesByOwnerIdAsync(string ownerId);
        Task<List<Vehicle>> GetAllWithDetailsAsync();
        Task<Vehicle> GetByIdWithDetailsAsync(int id);
        Task<IEnumerable<Vehicle>> GetFilteredVehiclesAsync(string userId, bool isClient, string searchString);
        Task<bool> ExistsByLicensePlateAsync(string licensePlate);
        Task<bool> IsInUseAsync(int id);
        Task<bool> ExistsByLicensePlateForEditAsync(int id, string licensePlate);
    }
}