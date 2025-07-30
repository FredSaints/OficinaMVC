using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMVC.Data.Entities;
using OficinaMVC.Data.Repositories;

namespace OficinaMVC.Controllers
{
    /// <summary>
    /// Controller for managing car brands. Only accessible by Admins.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class BrandsController : Controller
    {
        private readonly IBrandRepository _brandRepository;

        // The DataContext dependency is now removed.
        /// <summary>
        /// Initializes a new instance of the <see cref="BrandsController"/> class.
        /// </summary>
        /// <param name="brandRepository">Repository for brand data access.</param>
        public BrandsController(IBrandRepository brandRepository)
        {
            _brandRepository = brandRepository;
        }

        // GET: Brands/Index
        /// <summary>
        /// Displays a list of all brands.
        /// </summary>
        /// <returns>The brands index view.</returns>
        public async Task<IActionResult> Index()
        {
            var brands = await _brandRepository.GetAllAsync();
            return View(brands);
        }

        // GET: Brands/Create
        /// <summary>
        /// Displays the brand creation form.
        /// </summary>
        /// <returns>The create brand view.</returns>
        public IActionResult Create()
        {
            return View();
        }

        // POST: Brands/Create
        /// <summary>
        /// Handles brand creation POST requests.
        /// </summary>
        /// <param name="brand">The brand entity to create.</param>
        /// <returns>Redirects on success or returns the view with errors.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Brand brand)
        {
            if (ModelState.IsValid)
            {
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
        /// <summary>
        /// Displays the brand edit form for a given brand.
        /// </summary>
        /// <param name="id">The brand ID.</param>
        /// <returns>The edit brand view or not found.</returns>
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
        /// <summary>
        /// Handles brand edit POST requests.
        /// </summary>
        /// <param name="id">The brand ID.</param>
        /// <param name="brand">The brand entity with updated data.</param>
        /// <returns>Redirects on success or returns the view with errors.</returns>
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
        /// <summary>
        /// Displays the brand delete confirmation page.
        /// </summary>
        /// <param name="id">The brand ID.</param>
        /// <returns>The delete confirmation view or not found.</returns>
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
        /// <summary>
        /// Handles brand deletion POST requests.
        /// </summary>
        /// <param name="id">The brand ID.</param>
        /// <returns>Redirects to the brands index or shows an error if the brand has models.</returns>
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