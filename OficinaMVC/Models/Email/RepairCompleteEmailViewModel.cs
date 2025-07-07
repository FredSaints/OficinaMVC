namespace OficinaMVC.Models.Email
{
    public class RepairCompleteEmailViewModel
    {
        public string ClientFirstName { get; set; }
        public string VehicleDescription { get; set; }
        public string LicensePlate { get; set; }
        public int RepairId { get; set; }
        public string CompletionDate { get; set; }
    }
}