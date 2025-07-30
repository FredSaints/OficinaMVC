using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMVC.Data.Repositories;
using OficinaMVC.Helpers;
using OficinaMVC.Models.Mechanics;
using System.Linq;
using System.Threading.Tasks;

namespace OficinaMVC.Controllers
{
    /// <summary>
    /// Controller for managing mechanics, their specialties, and schedules. Accessible only by Admins.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class MechanicsController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly ISpecialtyRepository _specialtyRepository;
        private readonly IMechanicRepository _mechanicRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="MechanicsController"/> class.
        /// </summary>
        /// <param name="userHelper">Helper for user management operations.</param>
        /// <param name="specialtyRepository">Repository for specialty data access.</param>
        /// <param name="mechanicRepository">Repository for mechanic data access.</param>
        public MechanicsController(
            IUserHelper userHelper,
            ISpecialtyRepository specialtyRepository,
            IMechanicRepository mechanicRepository)
        {
            _userHelper = userHelper;
            _specialtyRepository = specialtyRepository;
            _mechanicRepository = mechanicRepository;
        }

        /// <summary>
        /// Displays a list of all mechanics.
        /// </summary>
        /// <returns>The mechanics index view.</returns>
        // GET: Mechanics
        public async Task<IActionResult> Index(string searchString, string searchType = "name")
        {
            var mechanics = await _userHelper.GetUsersInRoleAsync("Mechanic");

            if (!string.IsNullOrEmpty(searchString))
            {
                if (searchType == "nif")
                {
                    mechanics = mechanics.Where(m => m.NIF != null && m.NIF.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
                }
                else
                {
                    mechanics = mechanics.Where(m => m.FullName.StartsWith(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
                }
            }

            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentSearchType"] = searchType;

            return View(mechanics.OrderBy(m => m.FullName));
        }

        /// <summary>
        /// Displays the edit form for a mechanic, including specialties and schedules.
        /// </summary>
        /// <param name="id">The mechanic's user ID.</param>
        /// <returns>The edit mechanic view or not found.</returns>
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

        /// <summary>
        /// Handles mechanic edit POST requests.
        /// </summary>
        /// <param name="model">The mechanic edit view model.</param>
        /// <returns>Redirects on success or returns the view with errors.</returns>
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
