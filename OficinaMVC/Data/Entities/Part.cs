using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OficinaMVC.Data.Entities
{
    /// <summary>
    /// Represents a physical part or component used in repairs, tracked in inventory.
    /// </summary>
    public class Part : IEntity
    {
        /// <summary>
        /// The unique identifier for the part.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The name of the part (e.g., "Oil Filter", "Brake Pad Set").
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// An optional, more detailed description of the part.
        /// </summary>
        [MaxLength(250)]
        public string Description { get; set; }

        /// <summary>
        /// The current quantity of this part available in inventory.
        /// </summary>
        [Required]
        [Display(Name = "Stock Quantity")]
        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative.")]
        public int StockQuantity { get; set; } = 0;

        /// <summary>
        /// The selling price of a single unit of this part.
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Price must be greater than zero.")]
        public decimal Price { get; set; }

        /// <summary>
        /// Navigation property for the many-to-many relationship with <see cref="Repair"/>,
        /// detailing which repairs have used this part.
        /// </summary>
        public ICollection<RepairPart> RepairParts { get; set; } = new List<RepairPart>();
    }
}
