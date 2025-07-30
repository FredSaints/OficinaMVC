using OficinaMVC.Data.Entities;

namespace OficinaMVC.Data.Repositories
{
    /// <summary>
    /// Defines the contract for the repair repository, handling data operations for <see cref="Repair"/> entities.
    /// </summary>
    public interface IRepairRepository
    {
        /// <summary>
        /// Asynchronously gets a collection of all repairs, including detailed related entities.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of <see cref="Repair"/> entities with their details.</returns>
        Task<IEnumerable<Repair>> GetAllWithDetailsAsync();

        /// <summary>
        /// Asynchronously gets a single repair by its ID, including detailed related entities like vehicle, owner, parts, and mechanics.
        /// </summary>
        /// <param name="id">The unique identifier of the repair.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the detailed <see cref="Repair"/> if found; otherwise, null.</returns>
        Task<Repair> GetByIdWithDetailsAsync(int id);

        /// <summary>
        /// Asynchronously creates a new repair job from an existing appointment, marking the appointment as completed.
        /// </summary>
        /// <param name="appointmentId">The unique identifier of the appointment to convert.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the newly created <see cref="Repair"/> entity.</returns>
        /// <exception cref="System.InvalidOperationException">Thrown when the appointment is not found or a repair has already been started for it.</exception>
        Task<Repair> CreateRepairFromAppointmentAsync(int appointmentId);

        /// <summary>
        /// Asynchronously adds a specified quantity of a part to a repair. This operation is transactional, updating stock and total cost concurrently.
        /// </summary>
        /// <param name="repairId">The ID of the repair to add the part to.</param>
        /// <param name="partId">The ID of the part to add.</param>
        /// <param name="quantity">The number of units of the part to add.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the updated <see cref="Part"/> entity, allowing for a stock check.</returns>
        /// <exception cref="System.InvalidOperationException">Thrown if the repair/part is not found or if there is insufficient stock.</exception>
        Task<Part> AddPartToRepairAsync(int repairId, int partId, int quantity);

        /// <summary>
        /// Asynchronously removes a part entry from a repair. This operation is transactional, returning the part's quantity to stock and updating the repair's total cost.
        /// </summary>
        /// <param name="repairPartId">The unique identifier of the <see cref="RepairPart"/> join entity to remove.</param>
        /// <returns>A task that represents the asynchronous removal operation.</returns>
        /// <exception cref="System.InvalidOperationException">Thrown if the repair part entry is not found.</exception>
        Task RemovePartFromRepairAsync(int repairPartId);

        /// <summary>
        /// Asynchronously updates the status and description for a specific repair.
        /// </summary>
        /// <param name="repairId">The ID of the repair to update.</param>
        /// <param name="status">The new status for the repair.</param>
        /// <param name="description">The new description for the repair.</param>
        /// <returns>A task that represents the asynchronous update operation.</returns>
        Task UpdateRepairStatusAndNotesAsync(int repairId, string status, string description);

        /// <summary>
        /// Asynchronously deletes a repair and returns all its associated parts to the inventory stock.
        /// </summary>
        /// <param name="repairId">The ID of the repair to delete.</param>
        /// <returns>A task that represents the asynchronous deletion operation.</returns>
        Task DeleteRepairAndReturnPartsToStockAsync(int repairId);

        /// <summary>
        /// Asynchronously updates the list of mechanics assigned to a specific repair.
        /// </summary>
        /// <param name="repairId">The ID of the repair to update.</param>
        /// <param name="mechanicIds">A list of User IDs for the mechanics to be assigned.</param>
        /// <returns>A task that represents the asynchronous update operation.</returns>
        Task UpdateMechanicsForRepairAsync(int repairId, List<string> mechanicIds);

        /// <summary>
        /// Asynchronously gets a single <see cref="RepairPart"/> join entity by its unique identifier.
        /// </summary>
        /// <param name="repairPartId">The ID of the RepairPart entry.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="RepairPart"/> entity if found; otherwise, null.</returns>
        Task<RepairPart> GetRepairPartByIdAsync(int repairPartId);
    }
}
