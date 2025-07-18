using OficinaMVC.Data.Entities;
using System.Threading.Tasks;

namespace OficinaMVC.Data.Repositories
{
    public interface IPartRepository : IGenericRepository<Part>
    {
        Task<bool> ExistsByNameAsync(string name);
        Task<bool> ExistsForEditAsync(int id, string name);
        Task<bool> IsInUseAsync(int id);
    }
}