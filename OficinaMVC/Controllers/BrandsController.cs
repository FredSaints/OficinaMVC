using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OficinaMVC.Data;
using OficinaMVC.Data.Entities;
using OficinaMVC.Data.Repositories;

namespace OficinaMVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BrandsController : Controller
    {
        private readonly IBrandRepository _brandRepository;
        private readonly DataContext _context;

        public BrandsController(IBrandRepository brandRepository, DataContext context)
        {
            _brandRepository = brandRepository;
            _context = context;
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
                var exists = await _context.Brands.AnyAsync(b => b.Name == brand.Name);
                if (exists)
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
                var exists = await _context.Brands.AnyAsync(b => b.Name == brand.Name && b.Id != id);
                if (exists)
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