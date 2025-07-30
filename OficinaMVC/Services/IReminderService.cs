namespace OficinaMVC.Services
{
    /// <summary>
    /// Provides methods for sending appointment reminders to clients.
    /// </summary>
    public interface IReminderService
    {
        /// <summary>
        /// Sends reminder emails to clients with upcoming appointments.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SendAppointmentReminders();
    }
}
