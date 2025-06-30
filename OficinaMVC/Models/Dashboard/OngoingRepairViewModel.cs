namespace OficinaMVC.Models.Dashboard
{
    public class OngoingRepairViewModel
    {
        public int RepairId { get; set; }
        public string LicensePlate { get; set; }
        public string VehicleDescription { get; set; }
        public string ClientName { get; set; }
        public DateTime StartDate { get; set; }
    }
}
