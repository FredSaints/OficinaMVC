using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OficinaMVC.Data;
using OficinaMVC.Data.Entities;
using OficinaMVC.Data.Repositories;
using OficinaMVC.Models.Vehicles;

namespace OficinaMVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CarModelsController : Controller
    {
        private readonly ICarModelRepository _carModelRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly DataContext _context;

        public CarModelsController(ICarModelRepository carModelRepository, IBrandRepository brandRepository, DataContext context)
        {
            _carModelRepository = carModelRepository;
            _brandRepository = brandRepository;
            _context = context;
        }

        // GET: CarModels
        public async Task<IActionResult> Index()
        {
            var carModels = await _carModelRepository.GetAllWithBrandAsync();
            return View(carModels);
        }

        // GET: CarModels/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Brands = await _brandRepository.GetCombo();

            var viewModel = new CarModelViewModel();
            return View(viewModel);
        }

        // POST: CarModels/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CarModelViewModel viewModel)
        {
            var exists = await _context.CarModels.AnyAsync(m => m.Name == viewModel.Name && m.BrandId == viewModel.BrandId);
            if (exists)
            {
                ModelState.AddModelError("Name", "A model with this name already exists for this brand.");
            }

            if (ModelState.IsValid)
            {
                var carModel = new CarModel
                {
                    Name = viewModel.Name,
                    BrandId = viewModel.BrandId
                };
                await _carModelRepository.CreateAsync(carModel);
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Brands = await _brandRepository.GetCombo();
            return View(viewModel);
        }

        // GET: CarModels/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var carModel = await _carModelRepository.GetByIdAsync(id);
            if (carModel == null)
            {
                return NotFound();
            }

            ViewBag.Brands = await _brandRepository.GetCombo();

            var viewModel = new CarModelViewModel
            {
                Id = carModel.Id,
                Name = carModel.Name,
                BrandId = carModel.BrandId
            };
            return View(viewModel);
        }

        // POST: CarModels/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CarModelViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return BadRequest();
            }

            var exists = await _context.CarModels.AnyAsync(m => m.Name == viewModel.Name && m.BrandId == viewModel.BrandId && m.Id != id);
            if (exists)
            {
                ModelState.AddModelError("Name", "A model with this name already exists for this brand.");
            }

            if (ModelState.IsValid)
            {
                var carModel = new CarModel
                {
                    Id = viewModel.Id,
                    Name = viewModel.Name,
                    BrandId = viewModel.BrandId
                };
                await _carModelRepository.UpdateAsync(carModel);
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Brands = await _brandRepository.GetCombo();
            return View(viewModel);
        }

        // GET: CarModels/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var carModel = await _carModelRepository.GetByIdAsync(id);
            if (carModel == null)
            {
                return NotFound();
            }
            ViewBag.BrandName = (await _brandRepository.GetByIdAsync(carModel.BrandId))?.Name;
            return View(carModel);
        }

        // POST: CarModels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var carModel = await _carModelRepository.GetByIdAsync(id);
            if (carModel == null)
            {
                return NotFound();
            }

            var hasVehicles = await _context.Vehicles.AnyAsync(v => v.CarModelId == id);
            if (hasVehicles)
            {
                ViewData["ReturnController"] = "CarModels";
                ViewData["ReturnAction"] = "Index";
                ModelState.AddModelError(string.Empty, "This model cannot be deleted because it is assigned to one or more vehicles.");
                return View("DeleteConfirmationError", carModel);
            }

            await _carModelRepository.DeleteAsync(carModel);
            return RedirectToAction(nameof(Index));
        }
    }
}