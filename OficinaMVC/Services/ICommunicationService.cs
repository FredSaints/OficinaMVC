namespace OficinaMVC.Services
{
    /// <summary>
    /// Provides methods for handling communication-related background jobs, such as bulk appointment cancellation.
    /// </summary>
    public interface ICommunicationService
    {
        /// <summary>
        /// Cancels all upcoming appointments for a mechanic and notifies affected clients.
        /// </summary>
        /// <param name="mechanicId">The ID of the mechanic whose appointments will be cancelled.</param>
        /// <param name="connectionId">The SignalR connection ID for progress updates.</param>
        /// <param name="baseUrl">The base URL of the application for generating links.</param>
        /// <returns>The total number of appointments cancelled.</returns>
        Task<int> BulkCancelAppointmentsForMechanicAsync(string mechanicId, string connectionId, string baseUrl);
    }
}
