using OficinaMVC.Data.Entities;

namespace OficinaMVC.Data.Repositories
{
    public interface IInvoiceRepository
    {
        Task<Invoice> GetByRepairIdAsync(int repairId);
        Task<Invoice> GetByIdWithDetailsAsync(int invoiceId);
        Task<IEnumerable<Invoice>> GetAllWithDetailsAsync();
        Task<Invoice> GenerateInvoiceForRepairAsync(int repairId);
    }
}
