using Microsoft.EntityFrameworkCore;
using OficinaMVC.Data;
using OficinaMVC.Models.Home;

namespace OficinaMVC.Services
{
    /// <inheritdoc cref="IHomeService"/>
    public class HomeService : IHomeService
    {
        private readonly DataContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeService"/> class.
        /// </summary>
        /// <param name="context">The database context for data access.</param>
        public HomeService(DataContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task<HomeViewModel> GetHomeViewModelAsync()
        {
            var services = await _context.RepairTypes.ToListAsync();

            var mechanics = await _context.Users
                .Where(u => _context.UserRoles.Any(ur => ur.UserId == u.Id && _context.Roles.Any(r => r.Id == ur.RoleId && r.Name == "Mechanic")))
                .Include(u => u.UserSpecialties).ThenInclude(us => us.Specialty)
                .Select(u => new PublicMechanicViewModel
                {
                    FullName = u.FullName,
                    ProfileImageUrl = u.ProfileImageUrl ?? "/images/default-profile.png",
                    Specialties = u.UserSpecialties.Select(us => us.Specialty.Name).ToList()
                })
                .ToListAsync();

            var schedules = await _context.Schedules
                .GroupBy(s => s.DayOfWeek)
                .Select(g => new
                {
                    Day = g.Key,
                    StartTime = g.Min(s => s.StartTime),
                    EndTime = g.Max(s => s.EndTime)
                })
                .ToDictionaryAsync(k => k.Day, v => $"{v.StartTime:hh\\:mm} - {v.EndTime:hh\\:mm}");

            var viewModel = new HomeViewModel
            {
                Services = services,
                Mechanics = mechanics,
                OpeningHours = schedules
            };

            return viewModel;
        }
    }
}