using OficinaMVC.Data.Entities;

namespace OficinaMVC.Data.Repositories
{
    public interface IRepairTypeRepository : IGenericRepository<RepairType>
    {
        Task<bool> ExistsByNameAsync(string name);
        Task<bool> ExistsForEditAsync(int id, string name);
        Task<bool> IsInUseAsync(string typeName);
    }
}