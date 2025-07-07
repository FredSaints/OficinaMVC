using OficinaMVC.Data.Entities;

namespace OficinaMVC.Data.Repositories
{
    public interface IRepairRepository
    {
        Task<IEnumerable<Repair>> GetAllWithDetailsAsync();
        Task<Repair> GetByIdWithDetailsAsync(int id);
        Task<Repair> CreateRepairFromAppointmentAsync(int appointmentId);
        Task<Part> AddPartToRepairAsync(int repairId, int partId, int quantity);
        Task RemovePartFromRepairAsync(int repairPartId);
        Task UpdateRepairStatusAndNotesAsync(int repairId, string status, string description);
        Task DeleteRepairAndReturnPartsToStockAsync(int repairId);
        Task UpdateMechanicsForRepairAsync(int repairId, List<string> mechanicIds);
    }
}
