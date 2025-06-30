using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OficinaMVC.Data;
using OficinaMVC.Data.Entities;
using OficinaMVC.Data.Repositories;

namespace OficinaMVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RepairTypeController : Controller
    {
        private readonly IRepairTypeRepository _repository;
        private readonly DataContext _context;

        public RepairTypeController(IRepairTypeRepository repository, DataContext dataContext)
        {
            _repository = repository;
            _context = dataContext;
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
            var exists = await _context.RepairTypes.AnyAsync(rt => rt.Name == model.Name);
            if (exists)
            {
                ModelState.AddModelError("Name", "A repair type with this name already exists.");
            }

            if (ModelState.IsValid)
            {
                await _repository.CreateAsync(model);
                return RedirectToAction(nameof(Index));
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

            var exists = await _context.RepairTypes.AnyAsync(rt => rt.Name == model.Name && rt.Id != id);
            if (exists)
            {
                ModelState.AddModelError("Name", "A repair type with this name already exists.");
            }

            if (ModelState.IsValid)
            {
                await _repository.UpdateAsync(model);
                return RedirectToAction(nameof(Index));
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

            var isTypeUsed = await _context.Appointments.AnyAsync(a => a.ServiceType == type.Name);
            if (isTypeUsed)
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
