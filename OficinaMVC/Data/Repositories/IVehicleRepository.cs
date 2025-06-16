using OficinaMVC.Data.Entities;

namespace OficinaMVC.Data.Repositories
{
    public interface IVehicleRepository : IGenericRepository<Vehicle>
    {
        Task<List<Vehicle>> GetVehiclesByOwnerIdAsync(string ownerId);
    }
}
