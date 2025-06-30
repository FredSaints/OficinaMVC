using OficinaMVC.Data.Entities;

namespace OficinaMVC.Models.Invoices
{
    public class InvoiceDetailViewModel
    {
        public Invoice Invoice { get; set; }
        public bool IsEmail { get; set; } = false;
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyPhone { get; set; }
        public string CompanyNIF { get; set; }
    }
}
