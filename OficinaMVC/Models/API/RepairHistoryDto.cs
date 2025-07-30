namespace OficinaMVC.Models.API
{
    /// <summary>
    /// Data transfer object representing the history of a repair, including details, parts used, and mechanics involved.
    /// </summary>
    public class RepairHistoryDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the repair.
        /// </summary>
        public int RepairId { get; set; }

        /// <summary>
        /// Gets or sets the start date of the repair.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date of the repair, if available.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets or sets the status of the repair.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the description of the repair.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the total cost of the repair.
        /// </summary>
        public decimal TotalCost { get; set; }

        /// <summary>
        /// Gets or sets the list of parts used in the repair.
        /// </summary>
        public List<PartUsedDto> PartsUsed { get; set; } = new List<PartUsedDto>();

        /// <summary>
        /// Gets or sets the list of mechanics involved in the repair.
        /// </summary>
        public List<string> Mechanics { get; set; }
    }

    /// <summary>
    /// Data transfer object representing a part used in a repair.
    /// </summary>
    public class PartUsedDto
    {
        /// <summary>
        /// Gets or sets the name of the part.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the quantity of the part used.
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets the unit price of the part.
        /// </summary>
        public decimal UnitPrice { get; set; }
    }
}