using OficinaMVC.Data.Entities;

namespace OficinaMVC.Data.Repositories
{
    /// <summary>
    /// Defines the contract for the appointment repository, extending the generic repository
    /// with appointment-specific data access methods.
    /// </summary>
    public interface IAppointmentRepository : IGenericRepository<Appointment>
    {
        /// <summary>
        /// Gets a single appointment by its ID, including detailed related entities.
        /// </summary>
        /// <param name="appointmentId">The ID of the appointment to retrieve.</param>
        /// <returns>The <see cref="Appointment"/> with its client, mechanic, and vehicle details, or null if not found.</returns>
        Task<Appointment> GetByIdWithDetailsAsync(int appointmentId);

        /// <summary>
        /// Gets all appointments for a specific client, with an option to include completed and cancelled ones.
        /// </summary>
        /// <param name="clientId">The ID of the client.</param>
        /// <param name="includeCompleted">A flag to determine whether to include appointments with 'Completed' or 'Cancelled' status.</param>
        /// <returns>A collection of the client's appointments, sorted by status and then by date.</returns>
        Task<IEnumerable<Appointment>> GetByClientIdAsync(string clientId, bool includeCompleted);

        /// <summary>
        /// Gets a list of mechanics who are available at a specific date and time.
        /// </summary>
        /// <param name="appointmentDate">The date and time to check for availability.</param>
        /// <returns>A collection of <see cref="User"/> objects representing available mechanics.</returns>
        Task<IEnumerable<User>> GetAvailableMechanicsAsync(DateTime appointmentDate);

        /// <summary>
        /// Calculates which days in a given month are fully booked and thus unavailable for new appointments.
        /// </summary>
        /// <param name="year">The year of the month to check.</param>
        /// <param name="month">The month to check.</param>
        /// <returns>A collection of strings representing the unavailable dates in "yyyy-MM-dd" format.</returns>
        Task<IEnumerable<string>> GetUnavailableDaysAsync(int year, int month);

        /// <summary>
        /// Checks if a specific mechanic is scheduled to be working at a given date and time.
        /// </summary>
        /// <param name="mechanicId">The ID of the mechanic to check.</param>
        /// <param name="appointmentDate">The date and time to check against the mechanic's schedule.</param>
        /// <returns>True if the mechanic is scheduled to work; otherwise, false.</returns>
        Task<bool> IsMechanicAvailableAtTimeAsync(string mechanicId, DateTime appointmentDate);

        /// <summary>
        /// Creates a new appointment in the database and returns the created entity.
        /// </summary>
        /// <param name="appointment">The appointment entity to create.</param>
        /// <returns>The created <see cref="Appointment"/> entity with its new ID.</returns>
        Task<Appointment> CreateAndReturnAsync(Appointment appointment);
    }
}
