using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace OficinaMVC.Models.Appointments
{
    /// <summary>
    /// View model for creating or editing an appointment.
    /// </summary>
    public class AppointmentViewModel
    {
        /// <summary>
        /// Gets or sets the appointment ID.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the client ID for the appointment.
        /// </summary>
        [Required(ErrorMessage = "Please select a client.")]
        [Display(Name = "Client")]
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the list of available clients.
        /// </summary>
        public IEnumerable<SelectListItem> Clients { get; set; }

        /// <summary>
        /// Gets or sets the vehicle ID for the appointment.
        /// </summary>
        [Required(ErrorMessage = "Please select a vehicle.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid vehicle.")]
        [Display(Name = "Vehicle")]
        public int VehicleId { get; set; }

        /// <summary>
        /// Gets or sets the list of available vehicles.
        /// </summary>
        public IEnumerable<SelectListItem> Vehicles { get; set; }

        /// <summary>
        /// Gets or sets the service type ID for the appointment.
        /// </summary>
        [Required(ErrorMessage = "Please select a service type.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid service type.")]
        [Display(Name = "Service Type")]
        public int ServiceTypeId { get; set; }

        /// <summary>
        /// Gets or sets the list of available service types.
        /// </summary>
        public IEnumerable<SelectListItem> ServiceTypes { get; set; }

        /// <summary>
        /// Gets or sets the appointment date and time.
        /// </summary>
        [Required(ErrorMessage = "Please select an appointment date.")]
        [Display(Name = "Appointment Date & Time")]
        public DateTime AppointmentDate { get; set; }

        /// <summary>
        /// Gets or sets the mechanic ID for the appointment.
        /// </summary>
        [Required(ErrorMessage = "Please select a mechanic.")]
        [Display(Name = "Mechanic")]
        public string MechanicId { get; set; }

        /// <summary>
        /// Gets or sets the list of available mechanics.
        /// </summary>
        public IEnumerable<SelectListItem> Mechanics { get; set; }

        /// <summary>
        /// Gets or sets any notes for the appointment.
        /// </summary>
        [MaxLength(200)]
        public string Notes { get; set; }

        /// <summary>
        /// Gets or sets the display name of the client.
        /// </summary>
        [Display(Name = "Client")]
        public string ClientName { get; set; }

        /// <summary>
        /// Gets or sets the display information for the vehicle.
        /// </summary>
        [Display(Name = "Vehicle")]
        public string VehicleInfo { get; set; }
    }
}