using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OficinaMVC.Data.Entities
{
    public class Repair : IEntity
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Display(Name = "End Date")]
        public DateTime? EndDate { get; set; }

        [Required]
        [MaxLength(500)]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Total Cost")]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal TotalCost { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Ongoing";

        [Required]
        public int VehicleId { get; set; }
        [ForeignKey("VehicleId")]
        public Vehicle Vehicle { get; set; }

        public int? AppointmentId { get; set; }
        public Appointment Appointment { get; set; }

        public ICollection<User> Mechanics { get; set; } = new List<User>();

        public ICollection<RepairPart> RepairParts { get; set; } = new List<RepairPart>();
    }
}
