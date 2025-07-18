using Microsoft.AspNetCore.Mvc.Rendering;
using OficinaMVC.Data.Entities;

namespace OficinaMVC.Data.Repositories
{
    public interface ICarModelRepository : IGenericRepository<CarModel>
    {
        Task<IEnumerable<CarModel>> GetAllWithBrandAsync();
        Task<IEnumerable<SelectListItem>> GetCombo(int brandId);
        Task<bool> ExistsByNameAndBrandAsync(string name, int brandId);
        Task<bool> ExistsForEditAsync(int id, string name, int brandId);
        Task<bool> IsInUseAsync(int id);
    }
}