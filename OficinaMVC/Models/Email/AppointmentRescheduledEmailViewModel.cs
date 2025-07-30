namespace OficinaMVC.Models.Email
{
    /// <summary>
    /// ViewModel for the appointment rescheduled email notification.
    /// </summary>
    public class AppointmentRescheduledEmailViewModel
    {
        /// <summary>
        /// Gets or sets the first name of the client.
        /// </summary>
        public string ClientFirstName { get; set; }

        /// <summary>
        /// Gets or sets the description of the vehicle.
        /// </summary>
        public string VehicleDescription { get; set; }

        /// <summary>
        /// Gets or sets the license plate of the vehicle.
        /// </summary>
        public string LicensePlate { get; set; }

        /// <summary>
        /// Gets or sets the old appointment date as a string.
        /// </summary>
        public string OldAppointmentDate { get; set; }

        /// <summary>
        /// Gets or sets the new appointment date as a string.
        /// </summary>
        public string NewAppointmentDate { get; set; }
    }
}