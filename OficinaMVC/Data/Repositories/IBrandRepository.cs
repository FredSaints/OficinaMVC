using Microsoft.AspNetCore.Mvc.Rendering;
using OficinaMVC.Data.Entities;

namespace OficinaMVC.Data.Repositories
{
    public interface IBrandRepository : IGenericRepository<Brand>
    {
        Task<IEnumerable<SelectListItem>> GetCombo();
        Task<Brand> GetByIdWithModelsAsync(int id);
        Task<bool> ExistsByNameAsync(string name);
        Task<bool> ExistsForEditAsync(int id, string name);
    }
}