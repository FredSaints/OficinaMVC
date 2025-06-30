using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OficinaMVC.Data;
using OficinaMVC.Helpers;
using OficinaMVC.Models; // THE FIX: Added this using statement for ErrorViewModel
using OficinaMVC.Models.Home;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OficinaMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly DataContext _context;

        public HomeController(DataContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
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

            // 3. Get and consolidate Opening Hours ("Horários")
            var schedules = await _context.Schedules
                .GroupBy(s => s.DayOfWeek)
                .Select(g => new
                {
                    Day = g.Key,
                    StartTime = g.Min(s => s.StartTime),
                    EndTime = g.Max(s => s.EndTime)
                })
                .ToDictionaryAsync(k => k.Day, v => $"{v.StartTime:hh\\:mm} - {v.EndTime:hh\\:mm}");

            // Build the final view model
            var viewModel = new HomeViewModel
            {
                Services = services,
                Mechanics = mechanics,
                OpeningHours = schedules
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}