using OficinaMVC.Data.Entities;

namespace OficinaMVC.Data.Repositories
{
    /// <summary>
    /// Defines the contract for a generic repository for entities that implement <see cref="IEntity"/>.
    /// Provides a standard set of data access methods (CRUD operations).
    /// </summary>
    /// <typeparam name="T">The type of the entity. Must be a class and implement <see cref="IEntity"/>.</typeparam>
    public interface IGenericRepository<T> where T : class, IEntity
    {
        /// <summary>
        /// Gets a queryable collection of all entities, allowing for further filtering and projection before execution.
        /// </summary>
        /// <returns>An <see cref="IQueryable{T}"/> of all entities.</returns>
        IQueryable<T> GetAll();

        /// <summary>
        /// Asynchronously gets a collection of all entities.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of all entities.</returns>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Asynchronously gets a single entity by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the entity.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the entity if found; otherwise, null.</returns>
        Task<T> GetByIdAsync(int id);

        /// <summary>
        /// Asynchronously creates a new entity in the data store.
        /// </summary>
        /// <param name="entity">The entity to create.</param>
        /// <returns>A task that represents the asynchronous creation operation.</returns>
        Task CreateAsync(T entity);

        /// <summary>
        /// Asynchronously updates an existing entity in the data store.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <returns>A task that represents the asynchronous update operation.</returns>
        Task UpdateAsync(T entity);

        /// <summary>
        /// Asynchronously deletes an entity from the data store.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        /// <returns>A task that represents the asynchronous deletion operation.</returns>
        Task DeleteAsync(T entity);

        /// <summary>
        /// Asynchronously checks if an entity with the specified identifier exists.
        /// </summary>
        /// <param name="id">The unique identifier of the entity.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result is true if the entity exists; otherwise, false.
        /// </returns>
        Task<bool> ExistsAsync(int id);
    }
}
