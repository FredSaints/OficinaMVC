using OficinaMVC.Data.Entities;

namespace OficinaMVC.Data.Repositories
{
    public interface ISpecialtyRepository : IGenericRepository<Specialty>
    {
        Task<bool> ExistsByNameAsync(string name);
        Task<bool> ExistsForEditAsync(int id, string name);
        Task<bool> IsInUseAsync(int id);
    }
}
