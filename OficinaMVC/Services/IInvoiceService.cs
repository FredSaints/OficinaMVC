using OficinaMVC.Data.Entities;
using OficinaMVC.Helpers;
using OficinaMVC.Models.Invoices;

namespace OficinaMVC.Services
{
    /// <summary>
    /// Provides methods for generating, retrieving, and sending invoices.
    /// </summary>
    public interface IInvoiceService
    {
        /// <summary>
        /// Generates an invoice for a specific repair.
        /// </summary>
        /// <param name="repairId">The ID of the repair.</param>
        /// <returns>The generated invoice entity.</returns>
        Task<Invoice> GenerateForRepairAsync(int repairId);

        /// <summary>
        /// Gets the invoice detail view model for a specific invoice.
        /// </summary>
        /// <param name="invoiceId">The ID of the invoice.</param>
        /// <param name="isEmail">Whether the view model is for email purposes.</param>
        /// <returns>The invoice detail view model, or null if not found.</returns>
        Task<InvoiceDetailViewModel?> GetInvoiceDetailViewModelAsync(int invoiceId, bool isEmail = false);

        /// <summary>
        /// Sends the invoice by email to the client.
        /// </summary>
        /// <param name="invoiceId">The ID of the invoice to send.</param>
        /// <returns>The response indicating success or failure.</returns>
        Task<Response> SendInvoiceByEmailAsync(int invoiceId);
    }
}