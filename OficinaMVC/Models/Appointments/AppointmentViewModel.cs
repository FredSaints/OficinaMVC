using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace OficinaMVC.Models.Appointments
{
    public class AppointmentViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please select a client.")]
        [Display(Name = "Client")]
        public string ClientId { get; set; }
        public IEnumerable<SelectListItem> Clients { get; set; }

        [Required(ErrorMessage = "Please select a vehicle.")]
        [Display(Name = "Vehicle")]
        public int VehicleId { get; set; }
        public IEnumerable<SelectListItem> Vehicles { get; set; }

        [Required(ErrorMessage = "Please select a service type.")]
        [Display(Name = "Service Type")]
        public int ServiceTypeId { get; set; }
        public IEnumerable<SelectListItem> ServiceTypes { get; set; }

        [Required(ErrorMessage = "Please select an appointment date.")]
        [Display(Name = "Appointment Date & Time")]
        public DateTime AppointmentDate { get; set; }

        [Required(ErrorMessage = "Please select a mechanic.")]
        [Display(Name = "Mechanic")]
        public string MechanicId { get; set; }
        public IEnumerable<SelectListItem> Mechanics { get; set; }

        [MaxLength(200)]
        public string Notes { get; set; }

        [Display(Name = "Client")]
        public string ClientName { get; set; }

        [Display(Name = "Vehicle")]
        public string VehicleInfo { get; set; }
    }
}