using OficinaMVC.Data.Entities;

namespace OficinaMVC.Data.Repositories
{
    public interface IGenericRepository<T> where T : class, IEntity
    {
        IQueryable<T> GetAll();
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetByIdAsync(int id);
        Task CreateAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task<bool> ExistsAsync(int id);
    }
}
