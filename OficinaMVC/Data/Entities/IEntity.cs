namespace OficinaMVC.Data.Entities
{
    /// <summary>
    /// Defines a contract for entities that have a single integer primary key named 'Id'.
    /// </summary>
    /// <remarks>
    /// This interface is primarily used as a constraint in the generic repository (<see cref="Repositories.IGenericRepository{T}"/>)
    /// to ensure that all repository operations can rely on a common key structure.
    /// </remarks>
    public interface IEntity
    {
        /// <summary>
        /// Gets or sets the primary key for this entity.
        /// </summary>
        int Id { get; set; }
    }
}