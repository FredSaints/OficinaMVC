namespace OficinaMVC.Models.Dashboard
{
    public class AppointmentViewModel
    {
        public int Id { get; set; }
        public DateTime AppointmentTime { get; set; }
        public string ClientName { get; set; }
        public string VehicleInfo { get; set; }
        public string MechanicName { get; set; }
        public string ServiceType { get; set; }
    }
}
