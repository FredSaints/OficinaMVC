using OficinaMVC.Data.Entities;

namespace OficinaMVC.Data.Repositories
{
    public class PartRepository : GenericRepository<Part>, IPartRepository
    {
        private readonly DataContext _context;
        public PartRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public DataContext GetContext()
        {
            return _context;
        }
    }
}
