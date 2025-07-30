using Microsoft.EntityFrameworkCore;
using OficinaMVC.Data.Entities;

namespace OficinaMVC.Data.Repositories
{
    /// <summary>
    /// Repository for specialty-specific data access operations.
    /// </summary>
    public class SpecialtyRepository : GenericRepository<Specialty>, ISpecialtyRepository
    {
        private readonly DataContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpecialtyRepository"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        public SpecialtyRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public override async Task<IEnumerable<Specialty>> GetAllAsync()
        {
            return await _context.Specialties.AsNoTracking().ToListAsync();
        }

        /// <inheritdoc />
        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _context.Specialties.AnyAsync(s => s.Name == name);
        }

        /// <inheritdoc />
        public async Task<bool> ExistsForEditAsync(int id, string name)
        {
            return await _context.Specialties.AnyAsync(s => s.Name == name && s.Id != id);
        }

        /// <inheritdoc />
        public async Task<bool> IsInUseAsync(int id)
        {
            return await _context.UserSpecialties.AnyAsync(us => us.SpecialtyId == id);
        }
    }
}