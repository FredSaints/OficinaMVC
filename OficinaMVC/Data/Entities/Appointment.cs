using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OficinaMVC.Data.Entities
{
    /// <summary>
    /// Represents a scheduled appointment for a client's vehicle to receive a service.
    /// This entity serves as the precursor to a <see cref="Repair"/> job.
    /// </summary>
    public class Appointment : IEntity
    {
        /// <summary>
        /// The unique identifier for the appointment.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The scheduled date and time for the appointment.
        /// </summary>
        [Required]
        [Display(Name = "Start Date")]
        public DateTime Date { get; set; }

        /// <summary>
        /// The date and time when the appointment was concluded (e.g., when the repair job was started from it).
        /// </summary>
        [Display(Name = "Conclusion Date")]
        public DateTime? ConclusionDate { get; set; }

        /// <summary>
        /// A string representing the type of service requested for the appointment (e.g., "Oil Change", "Brake Inspection").
        /// This is typically linked to a <see cref="RepairType"/>.
        /// </summary>
        [Required]
        [MaxLength(50)]
        [Display(Name = "Service Type")]
        public string ServiceType { get; set; }

        /// <summary>
        /// The current status of the appointment (e.g., "Pending", "Completed", "Cancelled").
        /// </summary>
        [Required]
        [MaxLength(20)]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Pending";

        /// <summary>
        /// Optional notes from the receptionist or client regarding the appointment.
        /// </summary>
        [MaxLength(200)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        /// <summary>
        /// The foreign key for the client (<see cref="User"/>) who booked the appointment.
        /// </summary>
        [Required]
        public string ClientId { get; set; }

        /// <summary>
        /// Navigation property to the client <see cref="User"/> who owns this appointment.
        /// </summary>
        [ForeignKey("ClientId")]
        public User Client { get; set; }

        /// <summary>
        /// The foreign key for the mechanic (<see cref="User"/>) assigned to the appointment.
        /// </summary>
        [Required]
        public string MechanicId { get; set; }

        /// <summary>
        /// Navigation property to the assigned mechanic <see cref="User"/>.
        /// </summary>
        [ForeignKey("MechanicId")]
        public User Mechanic { get; set; }

        /// <summary>
        /// The foreign key for the <see cref="Vehicle"/> associated with this appointment.
        /// </summary>
        [Required]
        public int VehicleId { get; set; }

        /// <summary>
        /// Navigation property to the <see cref="Vehicle"/> for which the appointment is scheduled.
        /// </summary>
        [ForeignKey("VehicleId")]
        public Vehicle Vehicle { get; set; }

        /// <summary>
        /// The foreign key for the <see cref="Repair"/> job that was initiated from this appointment.
        /// This will be null until a repair is started.
        /// </summary>
        public int? RepairId { get; set; }

        /// <summary>
        /// Navigation property to the resulting <see cref="Repair"/> job.
        /// </summary>
        public Repair Repair { get; set; }
    }
}