using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMVC.Data.Repositories;
using OficinaMVC.Helpers;
using OficinaMVC.Models.Mechanics;
using System.Linq;
using System.Threading.Tasks;

namespace OficinaMVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class MechanicsController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly ISpecialtyRepository _specialtyRepository;
        private readonly IMechanicRepository _mechanicRepository;

        public MechanicsController(
            IUserHelper userHelper,
            ISpecialtyRepository specialtyRepository,
            IMechanicRepository mechanicRepository)
        {
            _userHelper = userHelper;
            _specialtyRepository = specialtyRepository;
            _mechanicRepository = mechanicRepository;
        }

        // GET: Mechanics
        public async Task<IActionResult> Index()
        {
            var users = await _userHelper.GetUsersInRoleAsync("Mechanic");
            return View(users);
        }

        // GET: Mechanics/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var mechanic = await _mechanicRepository.GetByIdWithDetailsAsync(id);
            if (mechanic == null)
            {
                return NotFound();
            }

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

        // POST: Mechanics/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MechanicEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableSpecialties = (await _specialtyRepository.GetAllAsync()).ToList();
                return View(model);
            }

            var (success, errorMessage) = await _mechanicRepository.UpdateMechanicAsync(model);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage);

                model.AvailableSpecialties = (await _specialtyRepository.GetAllAsync()).ToList();
                return View(model);
            }

            TempData["SuccessMessage"] = "Mechanic updated successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
