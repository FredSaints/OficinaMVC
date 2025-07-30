namespace OficinaMVC.Models.Email
{
    /// <summary>
    /// ViewModel for the appointment confirmed email notification.
    /// </summary>
    public class AppointmentConfirmedEmailViewModel
    {
        /// <summary>
        /// Gets or sets the first name of the client.
        /// </summary>
        public string ClientFirstName { get; set; }

        /// <summary>
        /// Gets or sets the appointment date as a string.
        /// </summary>
        public string AppointmentDate { get; set; }

        /// <summary>
        /// Gets or sets the type of service for the appointment.
        /// </summary>
        public string ServiceType { get; set; }

        /// <summary>
        /// Gets or sets the description of the vehicle.
        /// </summary>
        public string VehicleDescription { get; set; }

        /// <summary>
        /// Gets or sets the license plate of the vehicle.
        /// </summary>
        public string LicensePlate { get; set; }

        /// <summary>
        /// Gets or sets the name of the assigned mechanic.
        /// </summary>
        public string AssignedMechanic { get; set; }
    }
}