using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMVC.Data.Entities;
using OficinaMVC.Data.Repositories;

namespace OficinaMVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RepairTypeController : Controller
    {
        private readonly IRepairTypeRepository _repository;


        /// <summary>
        /// Initializes a new instance of the <see cref="RepairTypeController"/> class.
        /// </summary>
        /// <param name="repository">Repository for repair type data access.</param>
        public RepairTypeController(IRepairTypeRepository repository)
        {
            _repository = repository;
        }


        /// <summary>
        /// Displays a list of all repair types.
        /// </summary>
        /// <returns>The repair types index view.</returns>
        public async Task<IActionResult> Index()
        {
            var types = await _repository.GetAllAsync();
            return View(types);
        }


        /// <summary>
        /// Displays the repair type creation form.
        /// </summary>
        /// <returns>The create repair type view.</returns>
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Handles repair type creation POST requests.
        /// </summary>
        /// <param name="model">The repair type entity to create.</param>
        /// <returns>Redirects on success or returns the view with errors.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RepairType model)
        {
            if (ModelState.IsValid)
            {
                if (await _repository.ExistsByNameAsync(model.Name))
                {
                    ModelState.AddModelError("Name", "A repair type with this name already exists.");
                }
                else
                {
                    await _repository.CreateAsync(model);
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(model);
        }


        /// <summary>
        /// Displays the edit form for a repair type.
        /// </summary>
        /// <param name="id">The repair type ID.</param>
        /// <returns>The edit repair type view or not found.</returns>
        public async Task<IActionResult> Edit(int id)
        {
            var type = await _repository.GetByIdAsync(id);
            if (type == null) return NotFound();
            return View(type);
        }

        /// <summary>
        /// Handles repair type edit POST requests.
        /// </summary>
        /// <param name="id">The repair type ID.</param>
        /// <param name="model">The repair type entity with updated data.</param>
        /// <returns>Redirects on success or returns the view with errors.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RepairType model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                if (await _repository.ExistsForEditAsync(id, model.Name))
                {
                    ModelState.AddModelError("Name", "A repair type with this name already exists.");
                }
                else
                {
                    await _repository.UpdateAsync(model);
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(model);
        }


        /// <summary>
        /// Displays the delete confirmation page for a repair type.
        /// </summary>
        /// <param name="id">The repair type ID.</param>
        /// <returns>The delete confirmation view or not found.</returns>
        public async Task<IActionResult> Delete(int id)
        {
            var type = await _repository.GetByIdAsync(id);
            if (type == null) return NotFound();
            return View(type);
        }

        /// <summary>
        /// Handles repair type deletion POST requests.
        /// </summary>
        /// <param name="id">The repair type ID.</param>
        /// <returns>Redirects to the repair types index or shows an error if the type is in use.</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var type = await _repository.GetByIdAsync(id);
            if (type == null) return NotFound();

            if (await _repository.IsInUseAsync(type.Name))
            {
                ViewData["ReturnController"] = "RepairType";
                ViewData["ReturnAction"] = "Index";
                ModelState.AddModelError(string.Empty, "This repair type cannot be deleted because it has been used in one or more appointments.");
                return View("DeleteConfirmationError", type);
            }

            await _repository.DeleteAsync(type);
            return RedirectToAction(nameof(Index));
        }


        /// <summary>
        /// Displays the details of a specific repair type.
        /// </summary>
        /// <param name="id">The repair type ID.</param>
        /// <returns>The repair type details view or not found.</returns>
        public async Task<IActionResult> Details(int id)
        {
            var type = await _repository.GetByIdAsync(id);
            if (type == null) return NotFound();
            return View(type);
        }
    }
}