using OficinaMVC.Data.Entities;

namespace OficinaMVC.Data.Repositories
{
    public interface IPartRepository : IGenericRepository<Part>
    {
        DataContext GetContext();
    }
}
