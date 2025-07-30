namespace OficinaMVC.Models.Dashboard
{
    /// <summary>
    /// ViewModel representing an ongoing repair for the dashboard.
    /// </summary>
    public class OngoingRepairViewModel
    {
        /// <summary>
        /// Gets or sets the repair identifier.
        /// </summary>
        public int RepairId { get; set; }

        /// <summary>
        /// Gets or sets the license plate of the vehicle under repair.
        /// </summary>
        public string LicensePlate { get; set; }

        /// <summary>
        /// Gets or sets the description of the vehicle under repair.
        /// </summary>
        public string VehicleDescription { get; set; }

        /// <summary>
        /// Gets or sets the name of the client associated with the repair.
        /// </summary>
        public string ClientName { get; set; }

        /// <summary>
        /// Gets or sets the start date of the repair.
        /// </summary>
        public DateTime StartDate { get; set; }
    }
}
