using Microsoft.EntityFrameworkCore;
using OficinaMVC.Data.Entities;

namespace OficinaMVC.Data.Repositories
{
    /// <summary>
    /// Repository for handling data operations for <see cref="Part"/> entities.
    /// </summary>
    public class PartRepository : GenericRepository<Part>, IPartRepository
    {
        private readonly DataContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="PartRepository"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        public PartRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        /// <inheritdoc/>
        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _context.Parts.AnyAsync(p => p.Name == name);
        }

        /// <inheritdoc/>
        public async Task<bool> ExistsForEditAsync(int id, string name)
        {
            return await _context.Parts.AnyAsync(p => p.Name == name && p.Id != id);
        }

        /// <inheritdoc/>
        public async Task<bool> IsInUseAsync(int id)
        {
            return await _context.RepairParts.AnyAsync(rp => rp.PartId == id);
        }
    }
}