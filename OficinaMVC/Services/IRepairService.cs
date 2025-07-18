using OficinaMVC.Data.Entities;

namespace OficinaMVC.Services
{
    public interface IRepairService
    {
        Task<IEnumerable<Repair>> GetFilteredRepairsAsync(string status, string clientName, DateTime? startDate, DateTime? endDate);
        Task<Repair> CompleteRepairAsync(int repairId);
    }
}