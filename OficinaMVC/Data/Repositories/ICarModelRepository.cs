using Microsoft.AspNetCore.Mvc.Rendering;
using OficinaMVC.Data.Entities;

namespace OficinaMVC.Data.Repositories
{
    public interface ICarModelRepository : IGenericRepository<CarModel>
    {
        Task<IEnumerable<CarModel>> GetAllWithBrandAsync();
        Task<IEnumerable<SelectListItem>> GetCombo(int brandId);
    }
}
