using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OficinaMVC.Data;
using OficinaMVC.Data.Entities;
using OficinaMVC.Data.Repositories;
using OficinaMVC.Helpers;
using OficinaMVC.Hubs;
using OficinaMVC.Models.Appointments;
using OficinaMVC.Services;

namespace OficinaMVC.Controllers
{
    [Authorize]
    public class AppointmentController : Controller
    {
        private readonly IAppointmentRepository _appointmentRepo;
        private readonly IUserHelper _userHelper;
        private readonly IVehicleRepository _vehicleRepo;
        private readonly IRepairTypeRepository _repairTypeRepo;
        private readonly DataContext _context;
        private readonly IHubContext<NotificationHub, INotificationClient> _notificationHub;
        private readonly IMailHelper _mailHelper;
        private readonly IViewRendererService _viewRenderer;

        public AppointmentController(
            IAppointmentRepository appointmentRepo,
            IUserHelper userHelper,
            IVehicleRepository vehicleRepo,
            IRepairTypeRepository repairTypeRepo,
            DataContext context,
            IHubContext<NotificationHub, INotificationClient> notificationHub,
             IMailHelper mailHelper,
             IViewRendererService viewRenderer)
        {
            _appointmentRepo = appointmentRepo;
            _userHelper = userHelper;
            _vehicleRepo = vehicleRepo;
            _repairTypeRepo = repairTypeRepo;
            _context = context;
            _notificationHub = notificationHub;
            _mailHelper = mailHelper;
            _viewRenderer = viewRenderer;
        }

        // GET: Appointment/Index
        [Authorize(Roles = "Receptionist,Mechanic")]
        public async Task<IActionResult> Index(DateTime? filterDate, string status, string mechanicId)
        {
            IQueryable<Appointment> query = _appointmentRepo.GetAll()
                .Include(a => a.Client)
                .Include(a => a.Vehicle).ThenInclude(v => v.CarModel).ThenInclude(cm => cm.Brand)
                .Include(a => a.Mechanic);

            if (User.IsInRole("Mechanic"))
            {
                var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
                query = query.Where(a => a.MechanicId == user.Id);
            }

            var effectiveStatus = status ?? "Pending";

            if (effectiveStatus != "All")
            {
                query = query.Where(a => a.Status == effectiveStatus);
            }

            if (filterDate.HasValue)
            {
                query = query.Where(a => a.Date.Date == filterDate.Value.Date);
            }

            if (!User.IsInRole("Mechanic") && !string.IsNullOrEmpty(mechanicId))
            {
                query = query.Where(a => a.MechanicId == mechanicId);
            }

            ViewData["CurrentFilterDate"] = filterDate?.ToString("yyyy-MM-dd");
            ViewData["CurrentStatus"] = status;
            ViewData["CurrentMechanicId"] = mechanicId;

            var mechanics = await _userHelper.GetUsersInRoleAsync("Mechanic");
            ViewData["MechanicList"] = new SelectList(mechanics.OrderBy(m => m.FullName), "Id", "FullName", mechanicId);

            var appointments = await query.OrderByDescending(a => a.Date).ToListAsync();
            return View(appointments);
        }

        // GET: Appointment/Create
        [Authorize(Roles = "Receptionist")]
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Receptionist")]
        public async Task<IActionResult> Create(AppointmentViewModel model)
        {
            ModelState.Remove("ClientName");
            ModelState.Remove("VehicleInfo");
            ModelState.Remove("Clients");
            ModelState.Remove("Vehicles");
            ModelState.Remove("ServiceTypes");
            ModelState.Remove("Mechanics");

            if (model.AppointmentDate < DateTime.Now)
            {
                ModelState.AddModelError("AppointmentDate", "Cannot book an appointment in the past.");
            }

            if (ModelState.IsValid)
            {
                var serviceType = await _repairTypeRepo.GetByIdAsync(model.ServiceTypeId);
                if (serviceType != null)
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

                    var createdAppointment = await _context.Appointments
                        .Include(a => a.Client)
                        .Include(a => a.Mechanic)
                        .Include(a => a.Vehicle.CarModel.Brand)
                        .FirstOrDefaultAsync(a => a.Id == appointment.Id);

                    if (createdAppointment != null)
                    {
                        // --- 1. Notify the Mechanic (SignalR) ---
                        var mechanicMessage = $"New Assignment: A job for '{serviceType.Name}' has been assigned to you for {createdAppointment.Date:g}.";
                        var mechanicUrl = Url.Action("Index", "Appointment", new { filterDate = createdAppointment.Date.ToString("yyyy-MM-dd") });
                        var icon = "bi-calendar-plus";
                        await _notificationHub.Clients.User(model.MechanicId).ReceiveNotification(mechanicMessage, mechanicUrl, icon);

                        // --- 2. Email the Client ---
                        try
                        {
                            var emailViewModel = new Models.Email.AppointmentConfirmedEmailViewModel
                            {
                                ClientFirstName = createdAppointment.Client.FirstName,
                                AppointmentDate = createdAppointment.Date.ToString("dddd, MMMM dd, yyyy 'at' h:mm tt"),
                                ServiceType = createdAppointment.ServiceType,
                                VehicleDescription = $"{createdAppointment.Vehicle.CarModel.Brand.Name} {createdAppointment.Vehicle.CarModel.Name}",
                                LicensePlate = createdAppointment.Vehicle.LicensePlate,
                                AssignedMechanic = createdAppointment.Mechanic.FullName
                            };

                            var emailBody = await _viewRenderer.RenderToStringAsync("/Views/Shared/_EmailTemplates/AppointmentConfirmedEmail.cshtml", emailViewModel);
                            var subject = $"Appointment Confirmed - FredAuto Workshop";
                            _mailHelper.SendEmail(createdAppointment.Client.Email, subject, emailBody);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error sending confirmation email: {ex.Message}");
                            TempData["WarningMessage"] = "Appointment created, but the confirmation email could not be sent to the client.";
                        }
                    }

                    TempData["SuccessMessage"] = "Appointment created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("ServiceTypeId", "Invalid service type selected.");
            }

            await RepopulateDropdowns(model);
            return View(model);
        }

        [Authorize(Roles = "Receptionist")]
        public async Task<IActionResult> Edit(int id)
        {
            var appointment = await _appointmentRepo.GetAll()
                .Include(a => a.Client)
                .Include(a => a.Vehicle).ThenInclude(v => v.CarModel).ThenInclude(cm => cm.Brand)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null) return NotFound();

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Receptionist")]
        public async Task<IActionResult> Edit(int id, AppointmentViewModel model)
        {
            if (id != model.Id) return NotFound();

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
                var appointmentToUpdate = await _context.Appointments
                    .Include(a => a.Client)
                    .Include(a => a.Vehicle.CarModel.Brand)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (appointmentToUpdate == null) return NotFound();

                var serviceType = await _repairTypeRepo.GetByIdAsync(model.ServiceTypeId);
                if (serviceType != null)
                {
                    var originalDate = appointmentToUpdate.Date;
                    var originalMechanicId = appointmentToUpdate.MechanicId;

                    appointmentToUpdate.Date = model.AppointmentDate;
                    appointmentToUpdate.MechanicId = model.MechanicId;
                    appointmentToUpdate.Notes = model.Notes;
                    appointmentToUpdate.ServiceType = serviceType.Name;

                    await _appointmentRepo.UpdateAsync(appointmentToUpdate);
                    TempData["SuccessMessage"] = "Appointment updated successfully!";


                    if (originalDate != appointmentToUpdate.Date || originalMechanicId != appointmentToUpdate.MechanicId)
                    {
                        var oldDateStr = originalDate.ToString("dddd, MMMM dd 'at' h:mm tt");
                        var newDateStr = appointmentToUpdate.Date.ToString("dddd, MMMM dd 'at' h:mm tt");

                        var clientMessage = $"Update: Your appointment for {appointmentToUpdate.Vehicle.LicensePlate} has been rescheduled to {newDateStr}.";
                        var clientUrl = Url.Action("Index", "Home"); // Future enhancement: Link to a "My Appointments" page.
                        await _notificationHub.Clients.User(appointmentToUpdate.ClientId).ReceiveNotification(clientMessage, clientUrl, "bi-calendar-event");

                        var mechanicMessage = $"Update: Appointment for {appointmentToUpdate.Vehicle.LicensePlate} has been moved to {newDateStr}.";
                        var mechanicUrl = Url.Action("Index", "Dashboard");
                        await _notificationHub.Clients.User(appointmentToUpdate.MechanicId).ReceiveNotification(mechanicMessage, mechanicUrl, "bi-calendar-event");
                        if (originalMechanicId != appointmentToUpdate.MechanicId)
                        {
                            var oldMechanicMessage = $"Cancelled: The appointment for {appointmentToUpdate.Vehicle.LicensePlate} on {oldDateStr} has been reassigned.";
                            await _notificationHub.Clients.User(originalMechanicId).ReceiveNotification(oldMechanicMessage, mechanicUrl, "bi-calendar-x");
                        }

                        try
                        {
                            var emailViewModel = new Models.Email.AppointmentRescheduledEmailViewModel
                            {
                                ClientFirstName = appointmentToUpdate.Client.FirstName,
                                VehicleDescription = $"{appointmentToUpdate.Vehicle.CarModel.Brand.Name} {appointmentToUpdate.Vehicle.CarModel.Name}",
                                LicensePlate = appointmentToUpdate.Vehicle.LicensePlate,
                                OldAppointmentDate = oldDateStr,
                                NewAppointmentDate = newDateStr
                            };

                            var emailBody = await _viewRenderer.RenderToStringAsync("/Views/Shared/_EmailTemplates/AppointmentRescheduledEmail.cshtml", emailViewModel);
                            var subject = $"Appointment Rescheduled - FredAuto Workshop";
                            _mailHelper.SendEmail(appointmentToUpdate.Client.Email, subject, emailBody);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error sending appointment update email: {ex.Message}");
                            TempData["WarningMessage"] = "Appointment updated, but the confirmation email could not be sent.";
                        }
                    }

                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("ServiceTypeId", "Invalid service type selected.");
            }

            var originalAppointment = await _appointmentRepo.GetAll()
                .Include(a => a.Client)
                .Include(a => a.Vehicle).ThenInclude(v => v.CarModel).ThenInclude(cm => cm.Brand)
                .AsNoTracking().FirstOrDefaultAsync(a => a.Id == model.Id);
            if (originalAppointment != null)
            {
                model.ClientName = originalAppointment.Client.FullName;
                model.VehicleInfo = $"{originalAppointment.Vehicle.LicensePlate} ({originalAppointment.Vehicle.CarModel.Brand.Name} {originalAppointment.Vehicle.CarModel.Name})";
            }
            model.ServiceTypes = await GetServiceTypeSelectListAsync();
            model.Mechanics = await GetMechanicSelectListForEdit(model.AppointmentDate, model.MechanicId);
            return View(model);
        }

        [Authorize(Roles = "Receptionist")]
        public async Task<IActionResult> Delete(int id)
        {
            var appointment = await _appointmentRepo.GetAll()
               .Include(a => a.Client)
               .Include(a => a.Vehicle).ThenInclude(v => v.CarModel).ThenInclude(cm => cm.Brand)
               .Include(a => a.Mechanic)
               .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null) return NotFound();
            if (appointment.Status == "Completed")
            {
                TempData["ErrorMessage"] = "Cannot cancel a completed appointment.";
                return RedirectToAction(nameof(Index));
            }
            return View(appointment);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Receptionist")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appointment = await _appointmentRepo.GetAll()
               .Include(a => a.Client)
               .Include(a => a.Mechanic)
               .Include(a => a.Vehicle)
               .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment != null)
            {
                var appointmentDateStr = appointment.Date.ToString("dddd, MMMM dd 'at' h:mm tt");

                // 1.Notify the Mechanic (SignalR)
                var mechanicMessage = $"Cancelled: Your appointment for {appointment.Vehicle.LicensePlate} on {appointmentDateStr} has been cancelled.";
                var mechanicUrl = Url.Action("Index", "Dashboard");
                await _notificationHub.Clients.User(appointment.MechanicId).ReceiveNotification(mechanicMessage, mechanicUrl, "bi-calendar-x-fill text-danger");

                // 2. Notify the Client (SignalR)
                var clientMessage = $"Your appointment for {appointmentDateStr} has been cancelled by the workshop. Please contact us to reschedule.";
                var clientUrl = Url.Action("Index", "Home");
                await _notificationHub.Clients.User(appointment.ClientId).ReceiveNotification(clientMessage, clientUrl, "bi-calendar-x-fill text-danger");

                // 2. Send Email to the Client
                try
                {
                    var emailViewModel = new Models.Email.AppointmentCancelledEmailViewModel
                    {
                        ClientFirstName = appointment.Client.FirstName,
                        AppointmentDate = appointmentDateStr,
                        LicensePlate = appointment.Vehicle.LicensePlate
                    };

                    var emailBody = await _viewRenderer.RenderToStringAsync("/Views/Shared/_EmailTemplates/AppointmentCancelledEmail.cshtml", emailViewModel);

                    var subject = $"Appointment Cancellation Notice - FredAuto Workshop";
                    _mailHelper.SendEmail(appointment.Client.Email, subject, emailBody);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending cancellation email: {ex.Message}");
                }

                await _appointmentRepo.DeleteAsync(appointment);
                TempData["SuccessMessage"] = "Appointment has been successfully cancelled and all parties notified.";
            }
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
        [Authorize(Roles = "Receptionist")]
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
        [Authorize(Roles = "Receptionist")]
        public async Task<JsonResult> GetAvailableMechanics(DateTime appointmentDate)
        {
            if (appointmentDate.Date < DateTime.Today) return Json(new List<SelectListItem>());

            DayOfWeek dayOfWeek = appointmentDate.DayOfWeek;
            TimeSpan appointmentTime = appointmentDate.TimeOfDay;

            var allMechanics = await _userHelper.GetUsersInRoleAsync("Mechanic");
            var mechanicIds = allMechanics.Select(m => m.Id).ToList();

            var busyMechanicIds = await _appointmentRepo.GetAll()
                .Where(a => a.Date.Date == appointmentDate.Date && a.Status == "Pending")
                .Select(a => a.MechanicId)
                .Distinct().ToListAsync();

            var relevantSchedules = await _context.Schedules
                .Where(s => mechanicIds.Contains(s.UserId) && s.DayOfWeek == dayOfWeek)
                .ToListAsync();

            var availableMechanics = allMechanics.Where(mechanic =>
            {
                if (busyMechanicIds.Contains(mechanic.Id)) return false;
                return relevantSchedules.Any(s => s.UserId == mechanic.Id && appointmentTime >= s.StartTime && appointmentTime < s.EndTime);
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
        [Authorize(Roles = "Receptionist")]
        public async Task<JsonResult> GetUnavailableDays(int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            var allDaysInMonth = Enumerable.Range(1, endDate.Day)
                .Select(day => new DateTime(year, month, day)).ToList();

            var allSchedules = await _context.Schedules.AsNoTracking().ToListAsync();
            if (!allSchedules.Any()) return Json(allDaysInMonth.Select(d => d.ToString("yyyy-MM-dd")));

            var workingDaysOfWeek = allSchedules.Select(s => s.DayOfWeek).Distinct().ToList();

            var appointmentsInMonth = await _appointmentRepo.GetAll()
                .Where(a => a.Date.Year == year && a.Date.Month == month)
                .GroupBy(a => a.Date.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() }).ToListAsync();

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