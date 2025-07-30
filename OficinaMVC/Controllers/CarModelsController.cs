using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaMVC.Data.Entities;
using OficinaMVC.Data.Repositories;
using OficinaMVC.Models.Vehicles;

namespace OficinaMVC.Controllers
{
    /// <summary>
    /// Controller for managing car models. Only accessible by Admins.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class CarModelsController : Controller
    {
        private readonly ICarModelRepository _carModelRepository;
        private readonly IBrandRepository _brandRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="CarModelsController"/> class.
        /// </summary>
        /// <param name="carModelRepository">Repository for car model data access.</param>
        /// <param name="brandRepository">Repository for brand data access.</param>
        public CarModelsController(ICarModelRepository carModelRepository, IBrandRepository brandRepository)
        {
            _carModelRepository = carModelRepository;
            _brandRepository = brandRepository;
        }


        /// <summary>
        /// Displays a list of all car models with their brands.
        /// </summary>
        /// <returns>The car models index view.</returns>
        // GET: CarModels/Index
        public async Task<IActionResult> Index()
        {
            var carModels = await _carModelRepository.GetAllWithBrandAsync();
            return View(carModels);
        }


        /// <summary>
        /// Displays the car model creation form.
        /// </summary>
        /// <returns>The create car model view.</returns>
        // GET: CarModels/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Brands = await _brandRepository.GetCombo();
            var viewModel = new CarModelViewModel();
            return View(viewModel);
        }


        /// <summary>
        /// Handles car model creation POST requests.
        /// </summary>
        /// <param name="viewModel">The car model view model to create.</param>
        /// <returns>Redirects on success or returns the view with errors.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        // POST: CarModels/Create
        public async Task<IActionResult> Create(CarModelViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                if (await _carModelRepository.ExistsByNameAndBrandAsync(viewModel.Name, viewModel.BrandId))
                {
                    ModelState.AddModelError("Name", "A model with this name already exists for this brand.");
                }
                else
                {
                    var carModel = new CarModel
                    {
                        Name = viewModel.Name,
                        BrandId = viewModel.BrandId
                    };
                    await _carModelRepository.CreateAsync(carModel);
                    return RedirectToAction(nameof(Index));
                }
            }

            ViewBag.Brands = await _brandRepository.GetCombo();
            return View(viewModel);
        }


        /// <summary>
        /// Displays the car model edit form for a given car model.
        /// </summary>
        /// <param name="id">The car model ID.</param>
        /// <returns>The edit car model view or not found.</returns>
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

        /// <summary>
        /// Handles car model edit POST requests.
        /// </summary>
        /// <param name="id">The car model ID.</param>
        /// <param name="viewModel">The car model view model with updated data.</param>
        /// <returns>Redirects on success or returns the view with errors.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        // POST: CarModels/Edit/5
        public async Task<IActionResult> Edit(int id, CarModelViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                if (await _carModelRepository.ExistsForEditAsync(id, viewModel.Name, viewModel.BrandId))
                {
                    ModelState.AddModelError("Name", "A model with this name already exists for this brand.");
                }
                else
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
            }
            ViewBag.Brands = await _brandRepository.GetCombo();
            return View(viewModel);
        }

        /// <summary>
        /// Displays the car model delete confirmation page.
        /// </summary>
        /// <param name="id">The car model ID.</param>
        /// <returns>The delete confirmation view or not found.</returns>
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

        /// <summary>
        /// Handles car model deletion POST requests.
        /// </summary>
        /// <param name="id">The car model ID.</param>
        /// <returns>Redirects to the car models index or shows an error if the model is in use.</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        // POST: CarModels/Delete/5
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (await _carModelRepository.IsInUseAsync(id))
            {
                ViewData["ReturnController"] = "CarModels";
                ViewData["ReturnAction"] = "Index";
                ModelState.AddModelError(string.Empty, "This model cannot be deleted because it is assigned to one or more vehicles.");
                var carModelForError = await _carModelRepository.GetByIdAsync(id);
                return View("DeleteConfirmationError", carModelForError);
            }

            var carModel = await _carModelRepository.GetByIdAsync(id);
            if (carModel != null)
            {
                await _carModelRepository.DeleteAsync(carModel);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}