using OficinaMVC.Data.Entities;

namespace OficinaMVC.Data.Repositories
{
    public class RepairTypeRepository : GenericRepository<RepairType>, IRepairTypeRepository
    {
        public RepairTypeRepository(DataContext context) : base(context)
        {
        }
    }
}