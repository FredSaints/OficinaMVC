using OficinaMVC.Data.Entities;

namespace OficinaMVC.Data.Repositories
{
    /// <summary>
    /// Defines repository operations specific to the Vehicle entity.
    /// </summary>
    public interface IVehicleRepository : IGenericRepository<Vehicle>
    {
        /// <summary>
        /// Gets a list of vehicles owned by the specified owner.
        /// </summary>
        /// <param name="ownerId">The ID of the owner.</param>
        /// <returns>A list of vehicles belonging to the owner.</returns>
        Task<List<Vehicle>> GetVehiclesByOwnerIdAsync(string ownerId);
        /// <summary>
        /// Gets all vehicles with their related details.
        /// </summary>
        /// <returns>A list of vehicles with details.</returns>
        Task<List<Vehicle>> GetAllWithDetailsAsync();
        /// <summary>
        /// Gets a vehicle by its ID, including related details.
        /// </summary>
        /// <param name="id">The ID of the vehicle.</param>
        /// <returns>The vehicle with details, or null if not found.</returns>
        Task<Vehicle> GetByIdWithDetailsAsync(int id);
        /// <summary>
        /// Gets filtered vehicles based on user, client status, and search string.
        /// </summary>
        /// <param name="userId">The user ID for filtering.</param>
        /// <param name="isClient">Indicates if the user is a client.</param>
        /// <param name="searchString">The search string to filter vehicles.</param>
        /// <returns>An enumerable of filtered vehicles.</returns>
        Task<IEnumerable<Vehicle>> GetFilteredVehiclesAsync(string userId, bool isClient, string searchString);
        /// <summary>
        /// Checks if a vehicle exists with the specified license plate.
        /// </summary>
        /// <param name="licensePlate">The license plate to check.</param>
        /// <returns>True if a vehicle exists with the license plate; otherwise, false.</returns>
        Task<bool> ExistsByLicensePlateAsync(string licensePlate);
        /// <summary>
        /// Checks if the vehicle is currently in use.
        /// </summary>
        /// <param name="id">The ID of the vehicle.</param>
        /// <returns>True if the vehicle is in use; otherwise, false.</returns>
        Task<bool> IsInUseAsync(int id);
        /// <summary>
        /// Checks if a vehicle with the specified license plate exists for editing, excluding the specified ID.
        /// </summary>
        /// <param name="id">The ID to exclude from the check.</param>
        /// <param name="licensePlate">The license plate to check.</param>
        /// <returns>True if a vehicle with the license plate exists for another ID; otherwise, false.</returns>
        Task<bool> ExistsByLicensePlateForEditAsync(int id, string licensePlate);
    }
}