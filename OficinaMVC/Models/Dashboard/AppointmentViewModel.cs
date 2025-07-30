namespace OficinaMVC.Models.Dashboard
{
    /// <summary>
    /// ViewModel representing an appointment in the dashboard.
    /// </summary>
    public class AppointmentViewModel
    {
        /// <summary>
        /// Gets or sets the appointment identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the date and time of the appointment.
        /// </summary>
        public DateTime AppointmentTime { get; set; }

        /// <summary>
        /// Gets or sets the name of the client.
        /// </summary>
        public string ClientName { get; set; }

        /// <summary>
        /// Gets or sets the vehicle information.
        /// </summary>
        public string VehicleInfo { get; set; }

        /// <summary>
        /// Gets or sets the name of the mechanic assigned to the appointment.
        /// </summary>
        public string MechanicName { get; set; }

        /// <summary>
        /// Gets or sets the type of service for the appointment.
        /// </summary>
        public string ServiceType { get; set; }

        /// <summary>
        /// Gets or sets the related repair identifier, if any.
        /// </summary>
        public int? RepairId { get; set; }
    }
}
