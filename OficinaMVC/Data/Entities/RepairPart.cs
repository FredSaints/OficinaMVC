using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OficinaMVC.Data.Entities
{
    public class RepairPart : IEntity
    {
        public int Id { get; set; }

        [Required]
        public int RepairId { get; set; }
        public Repair Repair { get; set; }

        [Required]
        public int PartId { get; set; }
        public Part Part { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal UnitPrice { get; set; }
    }
}
