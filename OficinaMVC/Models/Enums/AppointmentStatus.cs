namespace OficinaMVC.Models.Enums
{
    /// <summary>
    /// Represents the status of an appointment.
    /// </summary>
    public enum AppointmentStatus
    {
        /// <summary>
        /// The appointment is pending and not yet confirmed.
        /// </summary>
        Pending,

        /// <summary>
        /// The appointment has been confirmed.
        /// </summary>
        Confirmed,

        /// <summary>
        /// The appointment is currently in progress.
        /// </summary>
        InProgress,

        /// <summary>
        /// The appointment has been completed.
        /// </summary>
        Completed,

        /// <summary>
        /// The appointment has been cancelled.
        /// </summary>
        Cancelled
    }
}
