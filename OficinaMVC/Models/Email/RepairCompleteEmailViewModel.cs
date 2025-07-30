namespace OficinaMVC.Models.Email
{
    /// <summary>
    /// ViewModel for the repair complete email notification.
    /// </summary>
    public class RepairCompleteEmailViewModel
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
        /// Gets or sets the repair identifier.
        /// </summary>
        public int RepairId { get; set; }

        /// <summary>
        /// Gets or sets the completion date as a string.
        /// </summary>
        public string CompletionDate { get; set; }
    }
}