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

        public RepairTypeController(IRepairTypeRepository repository)
        {
            _repository = repository;
        }

        public async Task<IActionResult> Index()
        {
            var types = await _repository.GetAllAsync();
            return View(types);
        }

        public IActionResult Create()
        {
            return View();
        }

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

        public async Task<IActionResult> Edit(int id)
        {
            var type = await _repository.GetByIdAsync(id);
            if (type == null) return NotFound();
            return View(type);
        }

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

        public async Task<IActionResult> Delete(int id)
        {
            var type = await _repository.GetByIdAsync(id);
            if (type == null) return NotFound();
            return View(type);
        }

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

        public async Task<IActionResult> Details(int id)
        {
            var type = await _repository.GetByIdAsync(id);
            if (type == null) return NotFound();
            return View(type);
        }
    }
}