using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OficinaMVC.Data;
using OficinaMVC.Data.Entities;
using OficinaMVC.Data.Repositories;

namespace OficinaMVC.Controllers
{
    namespace OficinaMVC.Controllers
    {
        [Authorize(Roles = "Mechanic,Receptionist")]
        public class PartsController : Controller
        {
            private readonly IPartRepository _partRepository;
            private readonly DataContext _context;

            public PartsController(IPartRepository partRepository, DataContext context)
            {
                _partRepository = partRepository;
                _context = context;
            }

            public async Task<IActionResult> Index()
            {
                var parts = await _partRepository.GetAllAsync();
                return View(parts);
            }

            public IActionResult Create()
            {
                return View();
            }

            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Create(Part part)
            {
                var exists = await _context.Parts.AnyAsync(p => p.Name == part.Name);
                if (exists)
                {
                    ModelState.AddModelError("Name", "A part with this name already exists.");
                }

                if (ModelState.IsValid)
                {
                    await _partRepository.CreateAsync(part);
                    TempData["SuccessMessage"] = "Part created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                return View(part);
            }

            public async Task<IActionResult> Edit(int id)
            {
                var part = await _partRepository.GetByIdAsync(id);
                if (part == null)
                {
                    return NotFound();
                }
                return View(part);
            }

            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Edit(int id, Part part)
            {
                if (id != part.Id)
                {
                    return NotFound();
                }

                var exists = await _context.Parts.AnyAsync(p => p.Name == part.Name && p.Id != id);
                if (exists)
                {
                    ModelState.AddModelError("Name", "A part with this name already exists.");
                }

                if (ModelState.IsValid)
                {
                    await _partRepository.UpdateAsync(part);
                    TempData["SuccessMessage"] = "Part updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                return View(part);
            }

            public async Task<IActionResult> Delete(int id)
            {
                var part = await _partRepository.GetByIdAsync(id);
                if (part == null)
                {
                    return NotFound();
                }
                return View(part);
            }

            [HttpPost, ActionName("Delete")]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> DeleteConfirmed(int id)
            {
                var part = await _partRepository.GetByIdAsync(id);
                if (part == null)
                {
                    return NotFound();
                }

                var context = _partRepository.GetContext();
                var isPartUsed = await context.RepairParts.AnyAsync(rp => rp.PartId == id);

                if (isPartUsed)
                {
                    ViewData["ReturnController"] = "Parts";
                    ViewData["ReturnAction"] = "Index";
                    ModelState.AddModelError(string.Empty, "This part cannot be deleted because it is part of a repair record.");
                    return View("DeleteConfirmationError", part);
                }

                await _partRepository.DeleteAsync(part);
                TempData["SuccessMessage"] = "Part deleted successfully.";
                return RedirectToAction(nameof(Index));
            }

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
}
