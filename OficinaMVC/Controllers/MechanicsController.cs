using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OficinaMVC.Data;
using OficinaMVC.Data.Entities;
using OficinaMVC.Data.Repositories;
using OficinaMVC.Models.Mechanics;

namespace OficinaMVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class MechanicsController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ISpecialtyRepository _specialtyRepository;
        private readonly DataContext _context;

        public MechanicsController(UserManager<User> userManager, ISpecialtyRepository specialtyRepository, DataContext context)
        {
            _userManager = userManager;
            _specialtyRepository = specialtyRepository;
            _context = context;
        }

        // GET: Mechanics
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.GetUsersInRoleAsync("Mechanic");
            return View(users);
        }

        // GET: Mechanics/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var mechanic = await _context.Users
                .Include(u => u.UserSpecialties)
                .ThenInclude(us => us.Specialty)
                .Include(u => u.Schedules)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (mechanic == null) return NotFound();

            var allSpecialties = await _specialtyRepository.GetAllAsync();

            var viewModel = new MechanicEditViewModel
            {
                UserId = mechanic.Id,
                FullName = mechanic.FullName,
                ProfileImageUrl = mechanic.ProfileImageUrl,
                SelectedSpecialtyIds = mechanic.UserSpecialties.Select(us => us.SpecialtyId).ToList(),
                AvailableSpecialties = allSpecialties.ToList(),
                Schedules = mechanic.Schedules?.Select(s => new ScheduleViewModel
                {
                    Id = s.Id,
                    DayOfWeek = s.DayOfWeek,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime
                }).ToList() ?? new List<ScheduleViewModel>()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MechanicEditViewModel model)
        {
            var mechanic = await _context.Users
                .Include(u => u.UserSpecialties)
                .Include(u => u.Schedules)
                .FirstOrDefaultAsync(u => u.Id == model.UserId);

            if (mechanic == null) return NotFound();

            // --- Update specialties ---
            var existingSpecialtyIds = mechanic.UserSpecialties.Select(us => us.SpecialtyId).ToList();
            var toAdd = model.SelectedSpecialtyIds.Except(existingSpecialtyIds).ToList();
            var toRemove = existingSpecialtyIds.Except(model.SelectedSpecialtyIds).ToList();

            foreach (var specialtyId in toAdd)
                mechanic.UserSpecialties.Add(new UserSpecialty { UserId = mechanic.Id, SpecialtyId = specialtyId });

            foreach (var specialtyId in toRemove)
            {
                var userSpecialty = mechanic.UserSpecialties.FirstOrDefault(us => us.SpecialtyId == specialtyId);
                if (userSpecialty != null)
                    mechanic.UserSpecialties.Remove(userSpecialty);
            }


            if (mechanic.Schedules != null && mechanic.Schedules.Any())
            {
                _context.Schedules.RemoveRange(mechanic.Schedules);
            }

            if (model.Schedules != null)
            {
                foreach (var sched in model.Schedules)
                {
                    _context.Schedules.Add(new Schedule
                    {
                        DayOfWeek = sched.DayOfWeek,
                        StartTime = sched.StartTime,
                        EndTime = sched.EndTime,
                        UserId = mechanic.Id
                    });
                }
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Mechanic updated successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
