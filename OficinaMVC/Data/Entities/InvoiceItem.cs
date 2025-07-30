using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OficinaMVC.Data.Entities
{
    /// <summary>
    /// Represents an item within an invoice, including description, quantity, and pricing details.
    /// </summary>
    public class InvoiceItem : IEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the invoice item.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the foreign key to the related invoice.
        /// </summary>
        [Required]
        public int InvoiceId { get; set; }

        /// <summary>
        /// Gets or sets the related invoice entity.
        /// </summary>
        public Invoice Invoice { get; set; }

        /// <summary>
        /// Gets or sets the description of the invoice item.
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the quantity of the item.
        /// </summary>
        [Required]
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets the unit price of the item.
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Gets the total price for the item (Quantity * UnitPrice).
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal TotalPrice => Quantity * UnitPrice;
    }
}