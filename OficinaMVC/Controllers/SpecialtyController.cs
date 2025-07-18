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

        public SpecialtyController(ISpecialtyRepository specialtyRepository)
        {
            _specialtyRepository = specialtyRepository;
        }

        // GET: Specialty
        public async Task<IActionResult> Index()
        {
            var specialties = await _specialtyRepository.GetAllAsync();
            return View(specialties);
        }

        // GET: Specialty/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Specialty/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
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
        public async Task<IActionResult> Edit(int id)
        {
            var specialty = await _specialtyRepository.GetByIdAsync(id);
            if (specialty == null) return NotFound();
            return View(specialty);
        }

        // POST: Specialty/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
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
        public async Task<IActionResult> Delete(int id)
        {
            var specialty = await _specialtyRepository.GetByIdAsync(id);
            if (specialty == null) return NotFound();
            return View(specialty);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
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