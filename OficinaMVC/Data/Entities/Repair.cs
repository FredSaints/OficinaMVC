using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OficinaMVC.Data.Entities
{
    /// <summary>
    /// Represents a repair job performed on a vehicle, tracking work, parts, and mechanics.
    /// </summary>
    public class Repair : IEntity
    {
        /// <summary>
        /// The unique identifier for the repair job.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The date and time when the repair job was started.
        /// </summary>
        [Required]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// The date and time when the repair job was completed. Null if ongoing.
        /// </summary>
        [Display(Name = "End Date")]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// A detailed description of the work performed, including diagnostics and actions taken.
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string Description { get; set; }

        /// <summary>
        /// The total cost of the repair, calculated from the sum of all parts used.
        /// </summary>
        [Required]
        [Display(Name = "Total Cost")]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal TotalCost { get; set; }

        /// <summary>
        /// The current status of the repair (e.g., "Ongoing", "Completed").
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Ongoing";

        /// <summary>
        /// The foreign key for the <see cref="Vehicle"/> being repaired.
        /// </summary>
        [Required]
        public int VehicleId { get; set; }
        /// <summary>
        /// Navigation property to the <see cref="Vehicle"/> associated with this repair.
        /// </summary>
        [ForeignKey("VehicleId")]
        public Vehicle Vehicle { get; set; }

        /// <summary>
        /// The optional foreign key for the <see cref="Appointment"/> that initiated this repair.
        /// </summary>
        public int? AppointmentId { get; set; }
        /// <summary>
        /// Navigation property to the originating <see cref="Appointment"/>.
        /// </summary>
        public Appointment Appointment { get; set; }

        /// <summary>
        /// A collection of mechanics (<see cref="User"/>) assigned to this repair job.
        /// </summary>
        public ICollection<User> Mechanics { get; set; } = new List<User>();

        /// <summary>
        /// A collection of <see cref="RepairPart"/> join entities, detailing the specific parts used in this repair.
        /// </summary>
        public ICollection<RepairPart> RepairParts { get; set; } = new List<RepairPart>();
    }
}
