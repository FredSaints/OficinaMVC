using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OficinaMVC.Data;
using OficinaMVC.Data.Entities;
using OficinaMVC.Data.Repositories;

namespace OficinaMVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SpecialtyController : Controller
    {
        private readonly ISpecialtyRepository _specialtyRepository;
        private readonly DataContext _context;

        public SpecialtyController(ISpecialtyRepository specialtyRepository, DataContext context)
        {
            _specialtyRepository = specialtyRepository;
            _context = context;
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
                await _specialtyRepository.CreateAsync(model);
                return RedirectToAction(nameof(Index));
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
                await _specialtyRepository.UpdateAsync(model);
                return RedirectToAction(nameof(Index));
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

            var hasReferences = await _context.UserSpecialties.AnyAsync(us => us.SpecialtyId == id);
            if (hasReferences)
            {
                ModelState.AddModelError("", "Cannot delete this specialty because it is assigned to one or more users.");
                return View(specialty);
            }

            try
            {
                await _specialtyRepository.DeleteAsync(specialty);
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Cannot delete this specialty because it is assigned to one or more users.");
                return View(specialty);
            }
        }
    }
}
