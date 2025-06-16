using Microsoft.EntityFrameworkCore;
using OficinaMVC.Data.Entities;

namespace OficinaMVC.Data.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class, IEntity
    {
        protected readonly DataContext _context;

        public GenericRepository(DataContext context)
        {
            _context = context;
        }

        public IQueryable<T> GetAll() => _context.Set<T>().AsNoTracking();

        public async Task<T> GetByIdAsync(int id) =>
            await _context.Set<T>().AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _context.Set<T>().AsNoTracking().ToListAsync();
        }

        public async Task CreateAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
            await SaveAllAsync();
        }

        public async Task UpdateAsync(T entity)
        {
            _context.Set<T>().Update(entity);
            await SaveAllAsync();
        }

        public async Task DeleteAsync(T entity)
        {
            _context.Set<T>().Remove(entity);
            await SaveAllAsync();
        }

        public async Task<bool> ExistsAsync(int id) =>
            await _context.Set<T>().AnyAsync(e => e.Id == id);

        private async Task<bool> SaveAllAsync() => await _context.SaveChangesAsync() > 0;
    }
}
