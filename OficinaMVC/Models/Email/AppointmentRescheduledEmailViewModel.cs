namespace OficinaMVC.Models.Email
{
    public class AppointmentRescheduledEmailViewModel
    {
        public string ClientFirstName { get; set; }
        public string VehicleDescription { get; set; }
        public string LicensePlate { get; set; }
        public string OldAppointmentDate { get; set; }
        public string NewAppointmentDate { get; set; }
    }
}