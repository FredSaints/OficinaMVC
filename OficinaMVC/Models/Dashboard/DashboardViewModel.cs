namespace OficinaMVC.Models.Dashboard
{
    public class DashboardViewModel
    {
        public int AppointmentsTodayCount { get; set; }
        public int OngoingRepairsCount { get; set; }
        public int LowStockPartsCount { get; set; }
        public List<AppointmentViewModel> TodaysAppointments { get; set; }
        public List<OngoingRepairViewModel> OngoingRepairs { get; set; }
        public List<LowStockPartViewModel> LowStockParts { get; set; }
        public ChartDataViewModel AppointmentsChartData { get; set; }
    }
}
