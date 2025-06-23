using OficinaMVC.Data.Entities;
using OficinaMVC.Models.Mechanics;

namespace OficinaMVC.Data.Repositories
{
    public interface IMechanicRepository
    {
        Task<User> GetByIdWithDetailsAsync(string mechanicId);
        Task<(bool Success, string ErrorMessage)> UpdateMechanicAsync(MechanicEditViewModel model);
    }
}
