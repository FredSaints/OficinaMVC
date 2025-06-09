using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OficinaMVC.Data.Entities
{
    public class Appointment : IEntity
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Start Date")]
        public DateTime Date { get; set; }

        [Display(Name = "Conclusion Date")]
        public DateTime? ConclusionDate { get; set; }

        [Required]
        [MaxLength(50)]
        [Display(Name = "Service Type")]
        public string ServiceType { get; set; }

        [Required]
        [MaxLength(20)]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Pending";

        [MaxLength(200)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        [Required]
        public string ClientId { get; set; }

        [ForeignKey("ClientId")]
        public User Client { get; set; }

        [Required]
        public string MechanicId { get; set; }

        [ForeignKey("MechanicId")]
        public User Mechanic { get; set; }

        [Required]
        public int VehicleId { get; set; }

        [ForeignKey("VehicleId")]
        public Vehicle Vehicle { get; set; }
    }
}