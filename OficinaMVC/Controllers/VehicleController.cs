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

        public VehicleController(IVehicleRepository vehicleRepo, IUserHelper userHelper)
        {
            _vehicleRepo = vehicleRepo;
            _userHelper = userHelper;
        }

        // GET: Vehicle
        public async Task<IActionResult> Index()
        {
            var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);

            if (User.IsInRole("Mechanic") || User.IsInRole("Receptionist"))
            {
                var vehicles = await _vehicleRepo.GetAllAsync();
                return View(vehicles);
            }
            else if (User.IsInRole("Client"))
            {
                var vehicles = await _vehicleRepo.GetVehiclesByOwnerIdAsync(user.Id);
                return View(vehicles);
            }

            return Forbid();
        }

        // GET: Vehicle/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var vehicle = await _vehicleRepo.GetByIdAsync(id);
            if (vehicle == null) return NotFound();

            var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
            if (User.IsInRole("Mechanic") || User.IsInRole("Receptionist") || (User.IsInRole("Client") && vehicle.OwnerId == user.Id))
                return View(vehicle);

            return Forbid();
        }

        // GET: Vehicle/Create
        [Authorize(Roles = "Mechanic,Receptionist")]
        public async Task<IActionResult> Create()
        {
            var model = new VehicleViewModel
            {
                OwnerList = await GetClientSelectListAsync()
            };
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Mechanic,Receptionist")]
        public async Task<IActionResult> Create(VehicleViewModel model)
        {
            if (!ModelState.IsValid)
            {

                foreach (var kv in ModelState)
                    foreach (var error in kv.Value.Errors)
                        System.Diagnostics.Debug.WriteLine($"{kv.Key}: {error.ErrorMessage}");

                model.OwnerList = await GetClientSelectListAsync();
                return View(model);
            }

            var vehicle = new Vehicle
            {
                LicensePlate = model.LicensePlate,
                Brand = model.Brand,
                CarModel = model.CarModel,
                Year = model.Year,
                FuelType = model.FuelType,
                OwnerId = model.OwnerId
            };

            try
            {
                await _vehicleRepo.CreateAsync(vehicle);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Vehicle NOT SAVED: " + ex.Message);
                ModelState.AddModelError("", "Could not save vehicle: " + ex.Message);
                model.OwnerList = await GetClientSelectListAsync();
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Vehicle/Edit/5
        [Authorize(Roles = "Mechanic,Receptionist")]
        public async Task<IActionResult> Edit(int id)
        {
            var vehicle = await _vehicleRepo.GetByIdAsync(id);
            if (vehicle == null) return NotFound();

            var model = new VehicleViewModel
            {
                Id = vehicle.Id,
                LicensePlate = vehicle.LicensePlate,
                Brand = vehicle.Brand,
                CarModel = vehicle.CarModel,
                Year = vehicle.Year,
                FuelType = vehicle.FuelType,
                OwnerId = vehicle.OwnerId,
                OwnerList = await GetClientSelectListAsync(vehicle.OwnerId)
            };
            return View(model);
        }

        // POST: Vehicle/Edit/5
        [HttpPost]
        [Authorize(Roles = "Mechanic,Receptionist")]
        public async Task<IActionResult> Edit(VehicleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.OwnerList = await GetClientSelectListAsync(model.OwnerId);
                return View(model);
            }

            var vehicle = await _vehicleRepo.GetByIdAsync(model.Id);
            if (vehicle == null) return NotFound();

            vehicle.LicensePlate = model.LicensePlate;
            vehicle.Brand = model.Brand;
            vehicle.CarModel = model.CarModel;
            vehicle.Year = model.Year;
            vehicle.FuelType = model.FuelType;
            vehicle.OwnerId = model.OwnerId;

            await _vehicleRepo.UpdateAsync(vehicle);
            return RedirectToAction(nameof(Index));
        }

        // GET: Vehicle/Delete/5
        [Authorize(Roles = "Mechanic,Receptionist")]
        public async Task<IActionResult> Delete(int id)
        {
            var vehicle = await _vehicleRepo.GetByIdAsync(id);
            if (vehicle == null) return NotFound();

            return View(vehicle);
        }

        // POST: Vehicle/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Mechanic,Receptionist")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vehicle = await _vehicleRepo.GetByIdAsync(id);
            if (vehicle == null) return NotFound();

            await _vehicleRepo.DeleteAsync(vehicle);
            return RedirectToAction(nameof(Index));
        }

        private async Task<List<SelectListItem>> GetClientSelectListAsync(string selectedOwnerId = null)
        {
            var clients = await _userHelper.GetUsersInRoleAsync("Client");
            return clients
                .Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = $"{u.FullName} ({u.Email})",
                    Selected = u.Id == selectedOwnerId
                })
                .ToList();
        }
    }
}