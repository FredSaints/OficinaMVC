using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMVC.Data.Entities;
using OficinaMVC.Data.Repositories;

namespace OficinaMVC.Controllers
{
    /// <summary>
    /// Controller for managing parts inventory. Accessible by mechanics and receptionists.
    /// </summary>
    [Authorize(Roles = "Mechanic,Receptionist")]
    public class PartsController : Controller
    {
        private readonly IPartRepository _partRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="PartsController"/> class.
        /// </summary>
        /// <param name="partRepository">Repository for part data access.</param>
        public PartsController(IPartRepository partRepository)
        {
            _partRepository = partRepository;
        }

        /// <summary>
        /// Displays a list of all parts in the inventory.
        /// </summary>
        /// <returns>The parts index view.</returns>
        public async Task<IActionResult> Index()
        {
            var parts = await _partRepository.GetAllAsync();
            return View(parts);
        }

        /// <summary>
        /// Displays the part creation form.
        /// </summary>
        /// <returns>The create part view.</returns>
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Handles part creation POST requests.
        /// </summary>
        /// <param name="part">The part entity to create.</param>
        /// <returns>Redirects on success or returns the view with errors.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Part part)
        {
            if (ModelState.IsValid)
            {
                if (await _partRepository.ExistsByNameAsync(part.Name))
                {
                    ModelState.AddModelError("Name", "A part with this name already exists.");
                }
                else
                {
                    await _partRepository.CreateAsync(part);
                    TempData["SuccessMessage"] = "Part created successfully!";
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(part);
        }

        /// <summary>
        /// Displays the part edit form for a given part.
        /// </summary>
        /// <param name="id">The part ID.</param>
        /// <returns>The edit part view or not found.</returns>
        public async Task<IActionResult> Edit(int id)
        {
            var part = await _partRepository.GetByIdAsync(id);
            if (part == null)
            {
                return NotFound();
            }
            return View(part);
        }

        /// <summary>
        /// Handles part edit POST requests.
        /// </summary>
        /// <param name="id">The part ID.</param>
        /// <param name="part">The part entity with updated data.</param>
        /// <returns>Redirects on success or returns the view with errors.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Part part)
        {
            if (id != part.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (await _partRepository.ExistsForEditAsync(id, part.Name))
                {
                    ModelState.AddModelError("Name", "A part with this name already exists.");
                }
                else
                {
                    await _partRepository.UpdateAsync(part);
                    TempData["SuccessMessage"] = "Part updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(part);
        }

        /// <summary>
        /// Displays the part delete confirmation page.
        /// </summary>
        /// <param name="id">The part ID.</param>
        /// <returns>The delete confirmation view or not found.</returns>
        public async Task<IActionResult> Delete(int id)
        {
            var part = await _partRepository.GetByIdAsync(id);
            if (part == null)
            {
                return NotFound();
            }
            return View(part);
        }

        /// <summary>
        /// Handles part deletion POST requests.
        /// </summary>
        /// <param name="id">The part ID.</param>
        /// <returns>Redirects to the parts index or shows an error if the part is in use.</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (await _partRepository.IsInUseAsync(id))
            {
                ViewData["ReturnController"] = "Parts";
                ViewData["ReturnAction"] = "Index";
                ModelState.AddModelError(string.Empty, "This part cannot be deleted because it is part of a repair record.");

                var partForErrorView = await _partRepository.GetByIdAsync(id);
                return View("DeleteConfirmationError", partForErrorView);
            }

            var partToDelete = await _partRepository.GetByIdAsync(id);
            if (partToDelete != null)
            {
                await _partRepository.DeleteAsync(partToDelete);
                TempData["SuccessMessage"] = "Part deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Returns part details as JSON for AJAX calls.
        /// </summary>
        /// <param name="id">The part ID.</param>
        /// <returns>A JSON result with part details or an error message.</returns>
        [HttpGet]
        public async Task<JsonResult> GetPartDetails(int id)
        {
            var part = await _partRepository.GetByIdAsync(id);
            if (part == null)
            {
                return Json(new { error = "Part not found." });
            }
            return Json(new { stockQuantity = part.StockQuantity, name = part.Name });
        }
    }
}