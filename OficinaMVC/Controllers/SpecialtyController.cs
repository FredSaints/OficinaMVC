using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMVC.Data.Entities;
using OficinaMVC.Data.Repositories;

namespace OficinaMVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SpecialtyController : Controller
    {
        private readonly ISpecialtyRepository _specialtyRepository;


        /// <summary>
        /// Initializes a new instance of the <see cref="SpecialtyController"/> class.
        /// </summary>
        /// <param name="specialtyRepository">Repository for specialty data access.</param>
        public SpecialtyController(ISpecialtyRepository specialtyRepository)
        {
            _specialtyRepository = specialtyRepository;
        }

        // GET: Specialty
        /// <summary>
        /// Displays a list of all specialties.
        /// </summary>
        /// <returns>The specialties index view.</returns>
        public async Task<IActionResult> Index()
        {
            var specialties = await _specialtyRepository.GetAllAsync();
            return View(specialties);
        }

        // GET: Specialty/Create
        /// <summary>
        /// Displays the specialty creation form.
        /// </summary>
        /// <returns>The create specialty view.</returns>
        public IActionResult Create()
        {
            return View();
        }

        // POST: Specialty/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        /// <summary>
        /// Handles specialty creation POST requests.
        /// </summary>
        /// <param name="model">The specialty entity to create.</param>
        /// <returns>Redirects on success or returns the view with errors.</returns>
        public async Task<IActionResult> Create(Specialty model)
        {
            if (ModelState.IsValid)
            {
                if (await _specialtyRepository.ExistsByNameAsync(model.Name))
                {
                    ModelState.AddModelError("Name", "A specialty with this name already exists.");
                }
                else
                {
                    await _specialtyRepository.CreateAsync(model);
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(model);
        }

        // GET: Specialty/Edit/5
        /// <summary>
        /// Displays the edit form for a specialty.
        /// </summary>
        /// <param name="id">The specialty ID.</param>
        /// <returns>The edit specialty view or not found.</returns>
        public async Task<IActionResult> Edit(int id)
        {
            var specialty = await _specialtyRepository.GetByIdAsync(id);
            if (specialty == null) return NotFound();
            return View(specialty);
        }

        // POST: Specialty/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        /// <summary>
        /// Handles specialty edit POST requests.
        /// </summary>
        /// <param name="id">The specialty ID.</param>
        /// <param name="model">The specialty entity with updated data.</param>
        /// <returns>Redirects on success or returns the view with errors.</returns>
        public async Task<IActionResult> Edit(int id, Specialty model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                if (await _specialtyRepository.ExistsForEditAsync(id, model.Name))
                {
                    ModelState.AddModelError("Name", "A specialty with this name already exists.");
                }
                else
                {
                    await _specialtyRepository.UpdateAsync(model);
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(model);
        }

        // GET: Specialty/Delete/5
        /// <summary>
        /// Displays the delete confirmation page for a specialty.
        /// </summary>
        /// <param name="id">The specialty ID.</param>
        /// <returns>The delete confirmation view or not found.</returns>
        public async Task<IActionResult> Delete(int id)
        {
            var specialty = await _specialtyRepository.GetByIdAsync(id);
            if (specialty == null) return NotFound();
            return View(specialty);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        /// <summary>
        /// Handles specialty deletion POST requests.
        /// </summary>
        /// <param name="id">The specialty ID.</param>
        /// <returns>Redirects to the specialties index or shows an error if the specialty is in use.</returns>
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var specialty = await _specialtyRepository.GetByIdAsync(id);
            if (specialty == null) return NotFound();

            if (await _specialtyRepository.IsInUseAsync(id))
            {
                ViewData["ReturnController"] = "Specialty";
                ViewData["ReturnAction"] = "Index";
                ModelState.AddModelError("", "Cannot delete this specialty because it is assigned to one or more users.");
                return View("DeleteConfirmationError", specialty);
            }

            await _specialtyRepository.DeleteAsync(specialty);
            return RedirectToAction(nameof(Index));
        }
    }
}