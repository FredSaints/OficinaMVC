using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMVC.Data.Entities;
using OficinaMVC.Data.Repositories;

namespace OficinaMVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BrandsController : Controller
    {
        private readonly IBrandRepository _brandRepository;

        // The DataContext dependency is now removed.
        public BrandsController(IBrandRepository brandRepository)
        {
            _brandRepository = brandRepository;
        }

        // GET: Brands
        public async Task<IActionResult> Index()
        {
            var brands = await _brandRepository.GetAllAsync();
            return View(brands);
        }

        // GET: Brands/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Brands/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Brand brand)
        {
            if (ModelState.IsValid)
            {
                // Use the new repository method for validation.
                if (await _brandRepository.ExistsByNameAsync(brand.Name))
                {
                    ModelState.AddModelError("Name", "A brand with this name already exists.");
                    return View(brand);
                }

                await _brandRepository.CreateAsync(brand);
                return RedirectToAction(nameof(Index));
            }
            return View(brand);
        }

        // GET: Brands/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var brand = await _brandRepository.GetByIdAsync(id);
            if (brand == null)
            {
                return NotFound();
            }
            return View(brand);
        }

        // POST: Brands/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Brand brand)
        {
            if (id != brand.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                // Use the new repository method for edit validation.
                if (await _brandRepository.ExistsForEditAsync(id, brand.Name))
                {
                    ModelState.AddModelError("Name", "A brand with this name already exists.");
                    return View(brand);
                }

                await _brandRepository.UpdateAsync(brand);
                return RedirectToAction(nameof(Index));
            }
            return View(brand);
        }

        // GET: Brands/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var brand = await _brandRepository.GetByIdAsync(id);
            if (brand == null)
            {
                return NotFound();
            }
            return View(brand);
        }

        // POST: Brands/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var brand = await _brandRepository.GetByIdWithModelsAsync(id);
            if (brand == null)
            {
                return NotFound();
            }

            if (brand.CarModels.Any())
            {
                ViewData["ReturnController"] = "Brands";
                ViewData["ReturnAction"] = "Index";
                ModelState.AddModelError(string.Empty, "This brand cannot be deleted because it has associated models. Please delete the models first.");
                return View("DeleteConfirmationError", brand);
            }

            await _brandRepository.DeleteAsync(brand);
            return RedirectToAction(nameof(Index));
        }
    }
}