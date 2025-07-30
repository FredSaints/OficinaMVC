namespace OficinaMVC.Models.Dashboard
{
    /// <summary>
    /// ViewModel representing a part with low stock for the dashboard.
    /// </summary>
    public class LowStockPartViewModel
    {
        /// <summary>
        /// Gets or sets the part identifier.
        /// </summary>
        public int PartId { get; set; }

        /// <summary>
        /// Gets or sets the name of the part.
        /// </summary>
        public string PartName { get; set; }

        /// <summary>
        /// Gets or sets the current stock quantity of the part.
        /// </summary>
        public int StockQuantity { get; set; }
    }
}
