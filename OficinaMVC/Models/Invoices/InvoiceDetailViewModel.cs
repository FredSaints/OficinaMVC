using OficinaMVC.Data.Entities;

namespace OficinaMVC.Models.Invoices
{
    /// <summary>
    /// ViewModel representing the details of an invoice for display or email purposes.
    /// </summary>
    public class InvoiceDetailViewModel
    {
        /// <summary>
        /// Gets or sets the invoice entity.
        /// </summary>
        public Invoice Invoice { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the view is for email.
        /// </summary>
        public bool IsEmail { get; set; } = false;

        /// <summary>
        /// Gets or sets the company name.
        /// </summary>
        public string CompanyName { get; set; }

        /// <summary>
        /// Gets or sets the company address.
        /// </summary>
        public string CompanyAddress { get; set; }

        /// <summary>
        /// Gets or sets the company phone number.
        /// </summary>
        public string CompanyPhone { get; set; }

        /// <summary>
        /// Gets or sets the company NIF (tax identification number).
        /// </summary>
        public string CompanyNIF { get; set; }
    }
}
