namespace OficinaMVC.Models.Email
{
    /// <summary>
    /// ViewModel for the appointment cancelled email notification.
    /// </summary>
    public class AppointmentCancelledEmailViewModel
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
        /// Gets or sets the license plate of the vehicle.
        /// </summary>
        public string LicensePlate { get; set; }
    }
}