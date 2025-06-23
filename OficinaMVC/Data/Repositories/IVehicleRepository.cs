using OficinaMVC.Data.Entities;

namespace OficinaMVC.Data.Repositories
{
    public interface IVehicleRepository : IGenericRepository<Vehicle>
    {
        Task<List<Vehicle>> GetVehiclesByOwnerIdAsync(string ownerId);
        Task<List<Vehicle>> GetAllWithDetailsAsync();
        Task<Vehicle> GetByIdWithDetailsAsync(int id);
    }
}
