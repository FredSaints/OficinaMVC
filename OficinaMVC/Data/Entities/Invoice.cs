using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OficinaMVC.Data.Entities
{
    /// <summary>
    /// Represents a financial document issued to a client after a repair is completed.
    /// It details the services rendered, parts used, and the total amount due.
    /// </summary>
    public class Invoice : IEntity
    {
        /// <summary>
        /// The unique identifier for the invoice.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The foreign key for the <see cref="Repair"/> job this invoice is associated with.
        /// </summary>
        [Required]
        public int RepairId { get; set; }
        /// <summary>
        /// Navigation property to the <see cref="Repair"/> that this invoice bills for.
        /// </summary>
        public Repair Repair { get; set; }

        /// <summary>
        /// The date and time when the invoice was generated.
        /// </summary>
        [Required]
        public DateTime InvoiceDate { get; set; }

        /// <summary>
        /// The total cost of all parts and labor before taxes are applied.
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        [Display(Name = "Subtotal")]
        public decimal Subtotal { get; set; }

        /// <summary>
        /// The amount of Value Added Tax (VAT) calculated on the subtotal.
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        [Display(Name = "VAT (23%)")]
        public decimal TaxAmount { get; set; }

        /// <summary>
        /// The final, total amount due for the invoice (Subtotal + TaxAmount).
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        [Display(Name = "Grand Total")]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// The current payment status of the invoice (e.g., "Unpaid", "Paid").
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Unpaid";

        /// <summary>
        /// A collection of individual line items that make up the invoice.
        /// </summary>
        public ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
    }
}