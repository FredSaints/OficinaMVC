using Microsoft.EntityFrameworkCore;
using OficinaMVC.Data.Entities;

namespace OficinaMVC.Data.Repositories
{
    /// <summary>
    /// Repository for repair type-specific data access operations.
    /// </summary>
    public class RepairTypeRepository : GenericRepository<RepairType>, IRepairTypeRepository
    {
        private readonly DataContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="RepairTypeRepository"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        public RepairTypeRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _context.RepairTypes.AnyAsync(rt => rt.Name == name);
        }

        /// <inheritdoc />
        public async Task<bool> ExistsForEditAsync(int id, string name)
        {
            return await _context.RepairTypes.AnyAsync(rt => rt.Name == name && rt.Id != id);
        }

        /// <inheritdoc />
        public async Task<bool> IsInUseAsync(string typeName)
        {
            return await _context.Appointments.AnyAsync(a => a.ServiceType == typeName);
        }
    }
}