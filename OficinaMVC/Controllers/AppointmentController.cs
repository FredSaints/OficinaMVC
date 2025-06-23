using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OficinaMVC.Data;
using OficinaMVC.Data.Entities;
using OficinaMVC.Data.Repositories;
using OficinaMVC.Helpers;
using OficinaMVC.Models.Appointments;

namespace OficinaMVC.Controllers
{
    [Authorize(Roles = "Receptionist")]
    public class AppointmentController : Controller
    {
        private readonly IAppointmentRepository _appointmentRepo;
        private readonly IUserHelper _userHelper;
        private readonly IVehicleRepository _vehicleRepo;
        private readonly IRepairTypeRepository _repairTypeRepo;
        private readonly DataContext _context;

        public AppointmentController(
            IAppointmentRepository appointmentRepo,
            IUserHelper userHelper,
            IVehicleRepository vehicleRepo,
            IRepairTypeRepository repairTypeRepo,
            DataContext context)
        {
            _appointmentRepo = appointmentRepo;
            _userHelper = userHelper;
            _vehicleRepo = vehicleRepo;
            _repairTypeRepo = repairTypeRepo;
            _context = context;
        }

        // GET: Appointment/Index
        public async Task<IActionResult> Index()
        {
            var appointments = await _appointmentRepo.GetAll()
                .Include(a => a.Client)
                .Include(a => a.Vehicle).ThenInclude(v => v.CarModel).ThenInclude(cm => cm.Brand)
                .Include(a => a.Mechanic)
                .ToListAsync();

            return View(appointments);
        }

        // GET: Appointment/Create
        public async Task<IActionResult> Create()
        {
            var model = new AppointmentViewModel
            {
                AppointmentDate = DateTime.Today.AddDays(1).Date.AddHours(9),
                Clients = await GetClientSelectListAsync(),
                ServiceTypes = await GetServiceTypeSelectListAsync(),
                Vehicles = new SelectList(Enumerable.Empty<SelectListItem>()),
                Mechanics = new SelectList(Enumerable.Empty<SelectListItem>())
            };
            return View(model);
        }

        // Post: Appointment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppointmentViewModel model)
        {
            ModelState.Remove("ClientName");
            ModelState.Remove("VehicleInfo");

            ModelState.Remove("Clients");
            ModelState.Remove("Vehicles");
            ModelState.Remove("ServiceTypes");
            ModelState.Remove("Mechanics");

            if (ModelState.IsValid)
            {
                var serviceType = await _repairTypeRepo.GetByIdAsync(model.ServiceTypeId);
                if (serviceType == null)
                {
                    ModelState.AddModelError("ServiceTypeId", "Invalid service type selected.");
                }
                else
                {
                    var appointment = new Appointment
                    {
                        ClientId = model.ClientId,
                        VehicleId = model.VehicleId,
                        ServiceType = serviceType.Name,
                        Date = model.AppointmentDate,
                        MechanicId = model.MechanicId,
                        Notes = model.Notes,
                        Status = "Pending"
                    };

                    await _appointmentRepo.CreateAsync(appointment);

                    TempData["SuccessMessage"] = "Appointment created successfully!";
                    return RedirectToAction(nameof(Index));
                }
            }
            await RepopulateDropdowns(model);
            return View(model);
        }

        // GET: Appointment/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var appointment = await _appointmentRepo.GetAll()
                .Include(a => a.Client)
                .Include(a => a.Vehicle).ThenInclude(v => v.CarModel).ThenInclude(cm => cm.Brand)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
            {
                return NotFound();
            }

            if (appointment.Status == "Completed")
            {
                TempData["ErrorMessage"] = "Cannot edit a completed appointment.";
                return RedirectToAction(nameof(Index));
            }

            var serviceType = await _repairTypeRepo.GetAll().FirstOrDefaultAsync(rt => rt.Name == appointment.ServiceType);

            var model = new AppointmentViewModel
            {
                Id = appointment.Id,
                ClientId = appointment.ClientId,
                VehicleId = appointment.VehicleId,
                ServiceTypeId = serviceType?.Id ?? 0,
                AppointmentDate = appointment.Date,
                MechanicId = appointment.MechanicId,
                Notes = appointment.Notes,

                ClientName = appointment.Client.FullName,
                VehicleInfo = $"{appointment.Vehicle.LicensePlate} ({appointment.Vehicle.CarModel.Brand.Name} {appointment.Vehicle.CarModel.Name})"
            };

            model.ServiceTypes = await GetServiceTypeSelectListAsync();
            model.Mechanics = await GetMechanicSelectListForEdit(model.AppointmentDate, model.MechanicId);

            return View(model);
        }

        // Post: Appointment/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AppointmentViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            ModelState.Remove("ClientId");
            ModelState.Remove("VehicleId");
            ModelState.Remove("ClientName");
            ModelState.Remove("VehicleInfo");
            ModelState.Remove("Clients");
            ModelState.Remove("Vehicles");
            ModelState.Remove("ServiceTypes");
            ModelState.Remove("Mechanics");

            if (ModelState.IsValid)
            {
                var appointmentToUpdate = await _appointmentRepo.GetByIdAsync(id);
                if (appointmentToUpdate == null) return NotFound();

                var serviceType = await _repairTypeRepo.GetByIdAsync(model.ServiceTypeId);
                if (serviceType == null)
                {
                    ModelState.AddModelError("ServiceTypeId", "Invalid service type selected.");
                }
                else
                {
                    appointmentToUpdate.Date = model.AppointmentDate;
                    appointmentToUpdate.MechanicId = model.MechanicId;
                    appointmentToUpdate.Notes = model.Notes;
                    appointmentToUpdate.ServiceType = serviceType.Name;

                    await _appointmentRepo.UpdateAsync(appointmentToUpdate);
                    
                    TempData["SuccessMessage"] = "Appointment updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
            }

            var originalAppointment = await _appointmentRepo.GetAll()
                .Include(a => a.Client)
                .Include(a => a.Vehicle).ThenInclude(v => v.CarModel).ThenInclude(cm => cm.Brand)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == model.Id);

            if (originalAppointment != null)
            {
                model.ClientName = originalAppointment.Client.FullName;
                model.VehicleInfo = $"{originalAppointment.Vehicle.LicensePlate} ({originalAppointment.Vehicle.CarModel.Brand.Name} {originalAppointment.Vehicle.CarModel.Name})";
            }

            model.ServiceTypes = await GetServiceTypeSelectListAsync();
            model.Mechanics = await GetMechanicSelectListForEdit(model.AppointmentDate, model.MechanicId);

            return View(model);
        }

        // GET: Appointment/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var appointment = await _appointmentRepo.GetAll()
                .Include(a => a.Client)
                .Include(a => a.Vehicle).ThenInclude(v => v.CarModel).ThenInclude(cm => cm.Brand)
                .Include(a => a.Mechanic)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
            {
                return NotFound();
            }

            if (appointment.Status == "Completed")
            {
                TempData["ErrorMessage"] = "Cannot cancel a completed appointment.";
                return RedirectToAction(nameof(Index));
            }

            return View(appointment);
        }

        // POST: Appointment/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appointment = await _appointmentRepo.GetByIdAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            await _appointmentRepo.DeleteAsync(appointment);
            TempData["SuccessMessage"] = "Appointment has been successfully cancelled.";
            return RedirectToAction(nameof(Index));
        }


        #region Private Helper Methods

        private async Task<IEnumerable<SelectListItem>> GetClientSelectListAsync()
        {
            var clients = await _userHelper.GetUsersInRoleAsync("Client");
            var list = clients.Select(u => new SelectListItem
            {
                Value = u.Id,
                Text = $"{u.FullName} ({u.Email})"
            }).OrderBy(t => t.Text).ToList();
            list.Insert(0, new SelectListItem { Value = "", Text = "Select a client..." });
            return list;
        }

        private async Task<IEnumerable<SelectListItem>> GetServiceTypeSelectListAsync()
        {
            var types = await _repairTypeRepo.GetAllAsync();
            var list = types.Select(t => new SelectListItem
            {
                Value = t.Id.ToString(),
                Text = t.Name
            }).OrderBy(t => t.Text).ToList();
            list.Insert(0, new SelectListItem { Value = "0", Text = "Select a service type..." });
            return list;
        }

        private async Task<IEnumerable<SelectListItem>> GetMechanicSelectListForEdit(DateTime appointmentDate, string currentMechanicId)
        {
            var mechanics = await _userHelper.GetUsersInRoleAsync("Mechanic");
            var list = mechanics.Select(m => new SelectListItem
            {
                Value = m.Id,
                Text = m.FullName,
                Selected = m.Id == currentMechanicId
            }).OrderBy(t => t.Text).ToList();

            list.Insert(0, new SelectListItem { Value = "", Text = "Select an available mechanic..." });
            return list;
        }

        private async Task RepopulateDropdowns(AppointmentViewModel model)
        {
            model.Clients = await GetClientSelectListAsync();
            model.ServiceTypes = await GetServiceTypeSelectListAsync();
            if (!string.IsNullOrEmpty(model.ClientId))
            {
                var vehicles = await _vehicleRepo.GetVehiclesByOwnerIdAsync(model.ClientId);
                model.Vehicles = vehicles.Select(v => new SelectListItem
                {
                    Value = v.Id.ToString(),
                    Text = $"{v.LicensePlate} ({v.CarModel.Brand.Name} {v.CarModel.Name})"
                });
            }
            else
            {
                model.Vehicles = new SelectList(Enumerable.Empty<SelectListItem>());
            }
            model.Mechanics = new SelectList(Enumerable.Empty<SelectListItem>());
        }

        #endregion

        #region API Methods for AJAX calls

        [HttpGet]
        public async Task<JsonResult> GetVehiclesByClient(string clientId)
        {
            if (string.IsNullOrEmpty(clientId))
            {
                return Json(new SelectList(Enumerable.Empty<SelectListItem>()));
            }
            var vehicles = await _vehicleRepo.GetVehiclesByOwnerIdAsync(clientId);
            var vehicleList = vehicles.Select(v => new SelectListItem
            {
                Value = v.Id.ToString(),
                Text = $"{v.LicensePlate} ({v.CarModel.Brand.Name} {v.CarModel.Name})"
            }).OrderBy(v => v.Text).ToList();
            return Json(new SelectList(vehicleList, "Value", "Text"));
        }

        [HttpGet]
        public async Task<JsonResult> GetAvailableMechanics(DateTime appointmentDate)
        {
            if (appointmentDate < DateTime.Now)
            {
                return Json(new List<SelectListItem>());
            }

            DayOfWeek dayOfWeek = appointmentDate.DayOfWeek;
            TimeSpan appointmentTime = appointmentDate.TimeOfDay;

            var allMechanics = await _userHelper.GetUsersInRoleAsync("Mechanic");
            var mechanicIds = allMechanics.Select(m => m.Id).ToList();

            var busyMechanicIds = await _appointmentRepo.GetAll()
                .Where(a => a.Date.Date == appointmentDate.Date)
                .Select(a => a.MechanicId)
                .Distinct()
                .ToListAsync();

            var relevantSchedules = await _context.Schedules
                .Where(s => mechanicIds.Contains(s.UserId) && s.DayOfWeek == dayOfWeek)
                .ToListAsync();

            var availableMechanics = allMechanics.Where(mechanic =>
            {
                if (busyMechanicIds.Contains(mechanic.Id))
                {
                    return false;
                }
                bool isScheduled = relevantSchedules
                    .Any(s => s.UserId == mechanic.Id && appointmentTime >= s.StartTime && appointmentTime < s.EndTime);
                return isScheduled;

            }).ToList();

            var mechanicList = availableMechanics.Select(m => new SelectListItem
            {
                Value = m.Id,
                Text = m.FullName
            }).OrderBy(m => m.Text).ToList();

            if (mechanicList.Any())
            {
                mechanicList.Insert(0, new SelectListItem { Value = "", Text = "Select an available mechanic..." });
            }

            return Json(mechanicList);
        }

        [HttpGet]
        public async Task<JsonResult> GetUnavailableDays(int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            var allDaysInMonth = Enumerable.Range(1, endDate.Day)
                                           .Select(day => new DateTime(year, month, day))
                                           .ToList();

            var allSchedules = await _context.Schedules.AsNoTracking().ToListAsync();
            if (!allSchedules.Any())
            {
                return Json(allDaysInMonth.Select(d => d.ToString("yyyy-MM-dd")));
            }

            var workingDaysOfWeek = allSchedules.Select(s => s.DayOfWeek).Distinct().ToList();

            var appointmentsInMonth = await _appointmentRepo.GetAll()
                .Where(a => a.Date.Year == year && a.Date.Month == month)
                .GroupBy(a => a.Date.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            var allMechanics = await _userHelper.GetUsersInRoleAsync("Mechanic");

            var unavailableDays = new List<string>();

            foreach (var day in allDaysInMonth)
            {
                if (!workingDaysOfWeek.Contains(day.DayOfWeek))
                {
                    unavailableDays.Add(day.ToString("yyyy-MM-dd"));
                    continue;
                }

                var appointmentsOnDay = appointmentsInMonth.FirstOrDefault(a => a.Date == day.Date);
                if (appointmentsOnDay != null && appointmentsOnDay.Count >= allMechanics.Count)
                {
                    unavailableDays.Add(day.ToString("yyyy-MM-dd"));
                }
            }

            return Json(unavailableDays);
        }

        #endregion
    }
}