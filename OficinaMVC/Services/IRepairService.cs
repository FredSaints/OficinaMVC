using OficinaMVC.Data.Entities;

namespace OficinaMVC.Services
{
    /// <summary>
    /// Provides methods for managing and completing repairs.
    /// </summary>
    public interface IRepairService
    {
        /// <summary>
        /// Gets a filtered list of repairs based on status, client name, and date range.
        /// </summary>
        /// <param name="status">The status to filter repairs by.</param>
        /// <param name="clientName">The client name to filter repairs by.</param>
        /// <param name="startDate">The start date for filtering repairs.</param>
        /// <param name="endDate">The end date for filtering repairs.</param>
        /// <returns>A collection of filtered repairs.</returns>
        Task<IEnumerable<Repair>> GetFilteredRepairsAsync(string status, string clientName, DateTime? startDate, DateTime? endDate);

        /// <summary>
        /// Marks a repair as completed.
        /// </summary>
        /// <param name="repairId">The ID of the repair to complete.</param>
        /// <returns>The completed repair entity.</returns>
        Task<Repair> CompleteRepairAsync(int repairId);
    }
}