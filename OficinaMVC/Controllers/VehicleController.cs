using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OficinaMVC.Data;
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
        private readonly DataContext _context;

        public VehicleController(
            IVehicleRepository vehicleRepo,
            IUserHelper userHelper,
            IBrandRepository brandRepo,
            ICarModelRepository carModelRepo,
            DataContext context)
        {
            _vehicleRepo = vehicleRepo;
            _userHelper = userHelper;
            _brandRepo = brandRepo;
            _carModelRepo = carModelRepo;
            _context = context;
        }

        // GET: Vehicle
        public async Task<IActionResult> Index(string searchString)
        {
            var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);

            IQueryable<Vehicle> query = _context.Vehicles
                .Include(v => v.Owner)
                .Include(v => v.CarModel)
                .ThenInclude(cm => cm.Brand);

            if (User.IsInRole("Client"))
            {
                query = query.Where(v => v.OwnerId == user.Id);
            }

            if (!String.IsNullOrEmpty(searchString))
            {
                query = query.Where(v => v.LicensePlate.Contains(searchString));
            }

            ViewData["CurrentFilter"] = searchString;

            var vehicles = await query.OrderBy(v => v.LicensePlate).ToListAsync();

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
            if (User.IsInRole("Mechanic") || User.IsInRole("Receptionist") || (User.IsInRole("Client") && vehicle.OwnerId == user.Id))
                return View(vehicle);

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
                if (vehicle.OwnerId != user.Id)
                {
                    return Forbid();
                }
            }
            return View(vehicle);
        }

        [Authorize(Roles = "Mechanic,Receptionist")]
        public async Task<IActionResult> Create()
        {
            var model = new VehicleViewModel
            {
                OwnerList = await GetClientSelectListAsync(),
                Brands = await _brandRepo.GetCombo(),
                CarModels = new SelectList(Enumerable.Empty<SelectListItem>())
            };
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Mechanic,Receptionist")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VehicleViewModel model)
        {
            var existingVehicle = await _context.Vehicles
                .FirstOrDefaultAsync(v => v.LicensePlate == model.LicensePlate);

            if (existingVehicle != null)
            {
                ModelState.AddModelError("LicensePlate", "A vehicle with this license plate already exists.");
            }

            ModelState.Remove("Brands");
            ModelState.Remove("CarModels");
            ModelState.Remove("OwnerList");

            if (!ModelState.IsValid)
            {
                await RepopulateDropdowns(model);
                return View(model);
            }

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
            TempData["SuccessMessage"] = "Vehicle created successfully!"; // Added for better UX
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Mechanic,Receptionist,Client")]
        public async Task<IActionResult> Edit(int id)
        {
            var vehicle = await _vehicleRepo.GetByIdWithDetailsAsync(id);
            if (vehicle == null) return NotFound();

            if (User.IsInRole("Client"))
            {
                var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
                if (vehicle.OwnerId != user.Id)
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

            if (!User.IsInRole("Client"))
            {
                await RepopulateDropdowns(model);
            }
            else
            {
                model.Brands = await _brandRepo.GetCombo();
                model.CarModels = await _carModelRepo.GetCombo(model.BrandId);
            }

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Mechanic,Receptionist,Client")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(VehicleViewModel model)
        {
            // The modelstate removals are still necessary
            ModelState.Remove("Brands");
            ModelState.Remove("CarModels");
            ModelState.Remove("OwnerList");

            if (!ModelState.IsValid)
            {
                // Repopulate dropdowns for both roles on failure
                await RepopulateDropdowns(model);
                return View(model);
            }

            var vehicle = await _vehicleRepo.GetByIdAsync(model.Id);
            if (vehicle == null) return NotFound();

            // --- SECURITY CHECK ---
            if (User.IsInRole("Client"))
            {
                var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
                if (vehicle.OwnerId != user.Id)
                {
                    return Forbid();
                }
            }
            // --- END SECURITY CHECK ---

            vehicle.LicensePlate = model.LicensePlate;
            vehicle.CarModelId = model.CarModelId;
            vehicle.Year = model.Year;
            vehicle.Mileage = model.Mileage;
            vehicle.FuelType = model.FuelType;

            // Only allow staff to change the owner
            if (!User.IsInRole("Client"))
            {
                vehicle.OwnerId = model.OwnerId;
            }

            await _vehicleRepo.UpdateAsync(vehicle);
            TempData["SuccessMessage"] = "Vehicle updated successfully!";
            return RedirectToAction(nameof(Index));
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
            if (vehicle == null)
            {
                return NotFound();
            }

            bool hasAppointments = await _context.Appointments.AnyAsync(a => a.VehicleId == id);
            bool hasRepairs = await _context.Repairs.AnyAsync(r => r.VehicleId == id);

            if (hasAppointments || hasRepairs)
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
            model.OwnerList = await GetClientSelectListAsync(model.OwnerId);
            model.Brands = await _brandRepo.GetCombo();
            model.CarModels = model.BrandId > 0
                ? await _carModelRepo.GetCombo(model.BrandId)
                : new SelectList(Enumerable.Empty<SelectListItem>());
        }
        #endregion
    }
}