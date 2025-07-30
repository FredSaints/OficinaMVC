using OficinaMVC.Data.Entities;

namespace OficinaMVC.Data.Repositories
{
    /// <summary>
    /// Defines the contract for the invoice repository, handling data operations for <see cref="Invoice"/> entities.
    /// </summary>
    public interface IInvoiceRepository
    {
        /// <summary>
        /// Asynchronously gets a single invoice by its unique identifier.
        /// </summary>
        /// <param name="invoiceId">The unique identifier of the invoice.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="Invoice"/> if found; otherwise, null.</returns>
        Task<Invoice> GetByIdAsync(int invoiceId);

        /// <summary>
        /// Asynchronously gets an invoice by the associated repair's unique identifier.
        /// </summary>
        /// <param name="repairId">The unique identifier of the associated repair.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="Invoice"/> if found; otherwise, null.</returns>
        Task<Invoice> GetByRepairIdAsync(int repairId);

        /// <summary>
        /// Asynchronously gets a single invoice by its ID, including detailed related entities like repair, vehicle, owner, and items.
        /// </summary>
        /// <param name="invoiceId">The unique identifier of the invoice.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the detailed <see cref="Invoice"/> if found; otherwise, null.</returns>
        Task<Invoice> GetByIdWithDetailsAsync(int invoiceId);

        /// <summary>
        /// Asynchronously gets a collection of all invoices with their details.
        /// </summary>
        /// <param name="showAll">If true, returns all invoices; if false, returns only unpaid invoices.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of <see cref="Invoice"/> entities.</returns>
        Task<IEnumerable<Invoice>> GetAllWithDetailsAsync(bool showAll = false);

        /// <summary>
        /// Asynchronously generates a new invoice for a completed repair. If an invoice already exists, it returns the existing one.
        /// </summary>
        /// <param name="repairId">The unique identifier of the completed repair.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the newly created or existing <see cref="Invoice"/>.</returns>
        /// <exception cref="System.Exception">Thrown when the repair is not found or is not in a 'Completed' status.</exception>
        Task<Invoice> GenerateInvoiceForRepairAsync(int repairId);

        /// <summary>
        /// Asynchronously updates an existing invoice in the data store.
        /// </summary>
        /// <param name="invoice">The invoice entity to update.</param>
        /// <returns>A task that represents the asynchronous update operation.</returns>
        Task UpdateInvoiceAsync(Invoice invoice);
    }
}
