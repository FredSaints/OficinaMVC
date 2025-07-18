using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OficinaMVC.Data.Entities;
using OficinaMVC.Data.Repositories;
using OficinaMVC.Helpers;
using OficinaMVC.Models.Vehicles;

namespace OficinaMVC.Controllers
{
    [Authorize]
    public class VehicleController : Controller
    {
        private readonly IVehicleRepository _vehicleRepo;
        private readonly IUserHelper _userHelper;
        private readonly IBrandRepository _brandRepo;
        private readonly ICarModelRepository _carModelRepo;

        public VehicleController(
            IVehicleRepository vehicleRepo,
            IUserHelper userHelper,
            IBrandRepository brandRepo,
            ICarModelRepository carModelRepo)
        {
            _vehicleRepo = vehicleRepo;
            _userHelper = userHelper;
            _brandRepo = brandRepo;
            _carModelRepo = carModelRepo;
        }

        // GET: Vehicle
        public async Task<IActionResult> Index(string searchString)
        {
            var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
            if (user == null)
            {
                return Unauthorized();
            }

            var isClient = User.IsInRole("Client");
            var vehicles = await _vehicleRepo.GetFilteredVehiclesAsync(user.Id, isClient, searchString);

            ViewData["CurrentFilter"] = searchString;

            var model = vehicles.Select(v => new VehicleListViewModel
            {
                Id = v.Id,
                LicensePlate = v.LicensePlate,
                Brand = v.CarModel?.Brand?.Name ?? "N/A",
                CarModel = v.CarModel?.Name ?? "N/A",
                Year = v.Year,
                Mileage = v.Mileage,
                FuelType = v.FuelType,
                OwnerName = v.Owner?.FullName ?? "N/A",
                OwnerEmail = v.Owner?.Email ?? "N/A"
            }).ToList();

            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var vehicle = await _vehicleRepo.GetByIdWithDetailsAsync(id);
            if (vehicle == null) return NotFound();

            var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
            if (user == null) return Forbid();

            if (User.IsInRole("Mechanic") || User.IsInRole("Receptionist") || (User.IsInRole("Client") && vehicle.OwnerId == user.Id))
            {
                return View(vehicle);
            }

            return Forbid();
        }

        // GET: Vehicle/History/5
        [Authorize(Roles = "Receptionist,Mechanic,Client")]
        public async Task<IActionResult> History(int id)
        {
            var vehicle = await _vehicleRepo.GetByIdWithDetailsAsync(id);
            if (vehicle == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Client"))
            {
                var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
                if (user == null || vehicle.OwnerId != user.Id)
                {
                    return Forbid();
                }
            }
            return View(vehicle);
        }

        [Authorize(Roles = "Mechanic,Receptionist")]
        public async Task<IActionResult> Create()
        {
            var model = new VehicleViewModel();
            await RepopulateDropdowns(model);
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Mechanic,Receptionist")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VehicleViewModel model)
        {
            ModelState.Remove("Brands");
            ModelState.Remove("CarModels");
            ModelState.Remove("OwnerList");

            if (ModelState.IsValid)
            {
                if (await _vehicleRepo.ExistsByLicensePlateAsync(model.LicensePlate))
                {
                    ModelState.AddModelError("LicensePlate", "A vehicle with this license plate already exists.");
                }
                else
                {
                    var vehicle = new Vehicle
                    {
                        LicensePlate = model.LicensePlate,
                        CarModelId = model.CarModelId,
                        Year = model.Year,
                        Mileage = model.Mileage,
                        FuelType = model.FuelType,
                        OwnerId = model.OwnerId
                    };

                    await _vehicleRepo.CreateAsync(vehicle);
                    TempData["SuccessMessage"] = "Vehicle created successfully!";
                    return RedirectToAction(nameof(Index));
                }
            }

            await RepopulateDropdowns(model);
            return View(model);
        }

        [Authorize(Roles = "Mechanic,Receptionist,Client")]
        public async Task<IActionResult> Edit(int id)
        {
            var vehicle = await _vehicleRepo.GetByIdWithDetailsAsync(id);
            if (vehicle == null) return NotFound();

            if (User.IsInRole("Client"))
            {
                var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
                if (user == null || vehicle.OwnerId != user.Id)
                {
                    return Forbid();
                }
            }

            var model = new VehicleViewModel
            {
                Id = vehicle.Id,
                LicensePlate = vehicle.LicensePlate,
                Year = vehicle.Year,
                Mileage = vehicle.Mileage,
                FuelType = vehicle.FuelType,
                OwnerId = vehicle.OwnerId,
                BrandId = vehicle.CarModel.BrandId,
                CarModelId = vehicle.CarModelId,
            };

            await RepopulateDropdowns(model);
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Mechanic,Receptionist,Client")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(VehicleViewModel model)
        {
            ModelState.Remove("Brands");
            ModelState.Remove("CarModels");
            ModelState.Remove("OwnerList");

            if (ModelState.IsValid)
            {
                if (await _vehicleRepo.ExistsByLicensePlateForEditAsync(model.Id, model.LicensePlate))
                {
                    ModelState.AddModelError("LicensePlate", "A vehicle with this license plate already exists.");
                }
                else
                {
                    var vehicle = await _vehicleRepo.GetByIdAsync(model.Id);
                    if (vehicle == null) return NotFound();

                    if (User.IsInRole("Client"))
                    {
                        var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
                        if (user == null || vehicle.OwnerId != user.Id)
                        {
                            return Forbid();
                        }
                    }

                    vehicle.LicensePlate = model.LicensePlate;
                    vehicle.CarModelId = model.CarModelId;
                    vehicle.Year = model.Year;
                    vehicle.Mileage = model.Mileage;
                    vehicle.FuelType = model.FuelType;

                    if (!User.IsInRole("Client"))
                    {
                        vehicle.OwnerId = model.OwnerId;
                    }

                    await _vehicleRepo.UpdateAsync(vehicle);
                    TempData["SuccessMessage"] = "Vehicle updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
            }
            await RepopulateDropdowns(model);
            return View(model);
        }

        [Authorize(Roles = "Mechanic,Receptionist")]
        public async Task<IActionResult> Delete(int id)
        {
            var vehicle = await _vehicleRepo.GetByIdWithDetailsAsync(id);
            if (vehicle == null) return NotFound();
            return View(vehicle);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Mechanic,Receptionist")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vehicle = await _vehicleRepo.GetByIdWithDetailsAsync(id);
            if (vehicle == null) return NotFound();

            if (await _vehicleRepo.IsInUseAsync(id))
            {
                ViewData["ReturnController"] = "Vehicle";
                ViewData["ReturnAction"] = "Index";
                ModelState.AddModelError(string.Empty, "This vehicle cannot be deleted because it has associated appointments or repair history.");
                return View("DeleteConfirmationError", vehicle);
            }

            await _vehicleRepo.DeleteAsync(vehicle);
            TempData["SuccessMessage"] = "Vehicle deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        #region API Methods
        [HttpGet]
        public async Task<JsonResult> GetCarModels(int brandId)
        {
            var carModels = await _carModelRepo.GetCombo(brandId);
            return Json(carModels);
        }
        #endregion

        #region Private Helper Methods
        private async Task<IEnumerable<SelectListItem>> GetClientSelectListAsync(string selectedOwnerId = null)
        {
            var clients = await _userHelper.GetUsersInRoleAsync("Client");
            return clients
                .Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = $"{u.FullName} ({u.Email})",
                    Selected = u.Id == selectedOwnerId
                })
                .OrderBy(u => u.Text)
                .ToList();
        }

        private async Task RepopulateDropdowns(VehicleViewModel model)
        {
            if (User.IsInRole("Client"))
            {
                model.Brands = await _brandRepo.GetCombo();
                model.CarModels = await _carModelRepo.GetCombo(model.BrandId);
            }
            else
            {
                model.OwnerList = await GetClientSelectListAsync(model.OwnerId);
                model.Brands = await _brandRepo.GetCombo();
                model.CarModels = model.BrandId > 0
                    ? await _carModelRepo.GetCombo(model.BrandId)
                    : new SelectList(Enumerable.Empty<SelectListItem>());
            }
        }
        #endregion
    }
}