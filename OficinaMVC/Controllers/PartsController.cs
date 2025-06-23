using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMVC.Data.Entities;
using OficinaMVC.Data.Repositories;

namespace OficinaMVC.Controllers
{
    namespace OficinaMVC.Controllers
    {
        [Authorize(Roles = "Admin,Receptionist")]
        public class PartsController : Controller
        {
            private readonly IPartRepository _partRepository;

            public PartsController(IPartRepository partRepository)
            {
                _partRepository = partRepository;
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

                // TODO: Add check here to prevent deleting a part that is used in a repair.

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
