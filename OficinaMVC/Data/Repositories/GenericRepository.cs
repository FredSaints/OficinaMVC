using Microsoft.EntityFrameworkCore;
using OficinaMVC.Data.Entities;

namespace OficinaMVC.Data.Repositories
{
    /// <summary>
    /// Provides a generic implementation of the <see cref="IGenericRepository{T}"/> interface.
    /// This class serves as a base for all other specific repositories, handling common CRUD operations.
    /// </summary>
    /// <typeparam name="T">The type of the entity. Must be a class and implement <see cref="IEntity"/>.</typeparam>
    public class GenericRepository<T> : IGenericRepository<T> where T : class, IEntity
    {
        protected readonly DataContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericRepository{T}"/> class.
        /// </summary>
        /// <param name="context">The database context to be used for data operations.</param>
        public GenericRepository(DataContext context)
        {
            _context = context;
        }

        /// <inheritdoc/>
        public IQueryable<T> GetAll() => _context.Set<T>().AsNoTracking();

        /// <inheritdoc/>
        public virtual async Task<T> GetByIdAsync(int id) =>
            await _context.Set<T>().AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);

        /// <inheritdoc/>
        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _context.Set<T>().AsNoTracking().ToListAsync();
        }

        /// <inheritdoc/>
        public async Task CreateAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
            await SaveAllAsync();
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(T entity)
        {
            _context.Set<T>().Update(entity);
            await SaveAllAsync();
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(T entity)
        {
            _context.Set<T>().Remove(entity);
            await SaveAllAsync();
        }

        /// <inheritdoc/>
        public async Task<bool> ExistsAsync(int id) =>
            await _context.Set<T>().AnyAsync(e => e.Id == id);

        /// <summary>
        /// Asynchronously saves all changes made in this context to the database.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous save operation. The task result is true if one or more
        /// objects were successfully saved to the database; otherwise, false.
        /// </returns>
        private async Task<bool> SaveAllAsync() => await _context.SaveChangesAsync() > 0;

 
    }
}
