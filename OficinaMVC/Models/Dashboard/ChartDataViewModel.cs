namespace OficinaMVC.Models.Dashboard
{
    /// <summary>
    /// ViewModel representing chart data for the dashboard.
    /// </summary>
    public class ChartDataViewModel
    {
        /// <summary>
        /// Gets or sets the labels for the chart.
        /// </summary>
        public List<string> Labels { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the data points for the chart.
        /// </summary>
        public List<int> Data { get; set; } = new List<int>();
    }
}
