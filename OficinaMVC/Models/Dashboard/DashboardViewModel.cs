namespace OficinaMVC.Models.Dashboard
{
    /// <summary>
    /// ViewModel representing the dashboard data.
    /// </summary>
    public class DashboardViewModel
    {
        /// <summary>
        /// Gets or sets the number of appointments scheduled for today.
        /// </summary>
        public int AppointmentsTodayCount { get; set; }

        /// <summary>
        /// Gets or sets the number of ongoing repairs.
        /// </summary>
        public int OngoingRepairsCount { get; set; }

        /// <summary>
        /// Gets or sets the number of parts with low stock.
        /// </summary>
        public int LowStockPartsCount { get; set; }

        /// <summary>
        /// Gets or sets the list of today's appointments.
        /// </summary>
        public List<AppointmentViewModel> TodaysAppointments { get; set; }

        /// <summary>
        /// Gets or sets the list of ongoing repairs.
        /// </summary>
        public List<OngoingRepairViewModel> OngoingRepairs { get; set; }

        /// <summary>
        /// Gets or sets the list of parts with low stock.
        /// </summary>
        public List<LowStockPartViewModel> LowStockParts { get; set; }

        /// <summary>
        /// Gets or sets the chart data for appointments.
        /// </summary>
        public ChartDataViewModel AppointmentsChartData { get; set; }
    }
}
