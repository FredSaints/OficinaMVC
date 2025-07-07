namespace OficinaMVC.Models.Email
{
    public class AppointmentConfirmedEmailViewModel
    {
        public string ClientFirstName { get; set; }
        public string AppointmentDate { get; set; }
        public string ServiceType { get; set; }
        public string VehicleDescription { get; set; }
        public string LicensePlate { get; set; }
        public string AssignedMechanic { get; set; }
    }
}