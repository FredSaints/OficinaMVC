using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OficinaMVC.Data.Entities
{
    /// <summary>
    /// Represents the join entity for the many-to-many relationship between <see cref="Repair"/> and <see cref="Part"/>.
    /// It records the quantity and price of a part used in a specific repair.
    /// </summary>
    public class RepairPart : IEntity
    {
        /// <summary>
        /// The unique identifier for this repair-part association.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The foreign key for the <see cref="Repair"/>.
        /// </summary>
        [Required]
        public int RepairId { get; set; }
        /// <summary>
        /// Navigation property to the <see cref="Repair"/>.
        /// </summary>
        public Repair Repair { get; set; }

        /// <summary>
        /// The foreign key for the <see cref="Part"/>.
        /// </summary>
        [Required]
        public int PartId { get; set; }
        /// <summary>
        /// Navigation property to the <see cref="Part"/>.
        /// </summary>
        public Part Part { get; set; }

        /// <summary>
        /// The number of units of the part used in this repair.
        /// </summary>
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        /// <summary>
        /// The price of the part at the time it was added to the repair.
        /// This "freezes" the price, protecting against future price changes for historical records.
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal UnitPrice { get; set; }
    }
}
