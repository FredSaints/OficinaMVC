using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
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
        private readonly IHubContext<NotificationHub, INotificationClient> _notificationHub;
        private readonly IMailHelper _mailHelper;
        private readonly IViewRendererService _viewRenderer;

        public AppointmentController(
            IAppointmentRepository appointmentRepo,
            IUserHelper userHelper,
            IVehicleRepository vehicleRepo,
            IRepairTypeRepository repairTypeRepo,
            IHubContext<NotificationHub, INotificationClient> notificationHub,
            IMailHelper mailHelper,
            IViewRendererService viewRenderer)
        {
            _appointmentRepo = appointmentRepo;
            _userHelper = userHelper;
            _vehicleRepo = vehicleRepo;
            _repairTypeRepo = repairTypeRepo;
            _notificationHub = notificationHub;
            _mailHelper = mailHelper;
            _viewRenderer = viewRenderer;
        }

        // GET: Appointment/
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
                if (user != null)
                {
                    query = query.Where(a => a.MechanicId == user.Id);
                }
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

        // GET: Appointment/My
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> MyAppointments(bool showCompleted = false)
        {
            var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
            if (user == null) return NotFound();

            var appointments = await _appointmentRepo.GetByClientIdAsync(user.Id, showCompleted);

            ViewData["ShowCompleted"] = showCompleted;
            return View("My", appointments);
        }

        // GET: Appointment/Create
        [Authorize(Roles = "Receptionist")]
        public async Task<IActionResult> Create()
        {
            var model = new AppointmentViewModel
            {
                AppointmentDate = DateTime.Today.AddDays(1).Date.AddHours(9)
            };
            await RepopulateDropdownsForCreateAsync(model);
            return View(model);
        }

        // POST: Appointment/Create
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
                if (!await _appointmentRepo.IsMechanicAvailableAtTimeAsync(model.MechanicId, model.AppointmentDate))
                {
                    ModelState.AddModelError("MechanicId", "The selected mechanic is not scheduled to work at the chosen time.");
                }
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

                    var createdAppointment = await _appointmentRepo.CreateAndReturnAsync(appointment);

                    TempData["SuccessMessage"] = "Appointment created successfully!";
                    await SendAppointmentCreationNotificationsAsync(createdAppointment.Id);

                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("ServiceTypeId", "Invalid service type selected.");
            }

            await RepopulateDropdownsForCreateAsync(model);
            return View(model);
        }

        // GET: Appointment/Edit/5
        [Authorize(Roles = "Receptionist")]
        public async Task<IActionResult> Edit(int id)
        {
            var appointment = await _appointmentRepo.GetByIdWithDetailsAsync(id);
            if (appointment == null) return NotFound();

            if (appointment.Status == "Completed")
            {
                TempData["ErrorMessage"] = "Cannot edit a completed appointment.";
                return RedirectToAction(nameof(Index));
            }

            var serviceType = await _repairTypeRepo.GetAll().AsNoTracking().FirstOrDefaultAsync(rt => rt.Name == appointment.ServiceType);
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

            await RepopulateDropdownsForEditAsync(model);
            return View(model);
        }

        // POST: Appointment/Edit/5
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
                if (!await _appointmentRepo.IsMechanicAvailableAtTimeAsync(model.MechanicId, model.AppointmentDate))
                {
                    ModelState.AddModelError("MechanicId", "The selected mechanic is not scheduled to work at the chosen time.");
                }
            }

            if (ModelState.IsValid)
            {
                var appointmentToUpdate = await _appointmentRepo.GetByIdAsync(id);
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

                    var updatedAppointmentWithDetails = await _appointmentRepo.GetByIdWithDetailsAsync(id);
                    await SendAppointmentUpdateNotificationsAsync(updatedAppointmentWithDetails, originalDate, originalMechanicId);

                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("ServiceTypeId", "Invalid service type selected.");
            }

            await RepopulateDropdownsForEditAsync(model);
            return View(model);
        }

        // GET: Appointment/Delete/5
        [Authorize(Roles = "Receptionist")]
        public async Task<IActionResult> Delete(int id)
        {
            var appointment = await _appointmentRepo.GetByIdWithDetailsAsync(id);
            if (appointment == null) return NotFound();

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
        [Authorize(Roles = "Receptionist")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appointment = await _appointmentRepo.GetByIdWithDetailsAsync(id);
            if (appointment != null)
            {
                await SendAppointmentCancellationNotificationsAsync(appointment);
                await _appointmentRepo.DeleteAsync(appointment);
                TempData["SuccessMessage"] = "Appointment has been successfully cancelled and all parties notified.";
            }
            return RedirectToAction(nameof(Index));
        }

        #region Private Helper Methods

        private async Task SendAppointmentCreationNotificationsAsync(int appointmentId)
        {
            var appointment = await _appointmentRepo.GetByIdWithDetailsAsync(appointmentId);
            if (appointment == null) return;

            var mechanicMessage = $"New Assignment: A job for '{appointment.ServiceType}' has been assigned to you for {appointment.Date:g}.";
            var mechanicUrl = Url.Action("Index", "Appointment", new { filterDate = appointment.Date.ToString("yyyy-MM-dd") });
            await _notificationHub.Clients.User(appointment.MechanicId).ReceiveNotification(mechanicMessage, mechanicUrl, "bi-calendar-plus");

            try
            {
                var emailViewModel = new Models.Email.AppointmentConfirmedEmailViewModel
                {
                    ClientFirstName = appointment.Client.FirstName,
                    AppointmentDate = appointment.Date.ToString("dddd, MMMM dd, yyyy 'at' h:mm tt"),
                    ServiceType = appointment.ServiceType,
                    VehicleDescription = $"{appointment.Vehicle.CarModel.Brand.Name} {appointment.Vehicle.CarModel.Name}",
                    LicensePlate = appointment.Vehicle.LicensePlate,
                    AssignedMechanic = appointment.Mechanic.FullName
                };
                var emailBody = await _viewRenderer.RenderToStringAsync("/Views/Shared/_EmailTemplates/AppointmentConfirmedEmail.cshtml", emailViewModel);
                _mailHelper.SendEmail(appointment.Client.Email, "Appointment Confirmed - FredAuto Workshop", emailBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending confirmation email: {ex.Message}");
                TempData["WarningMessage"] = "Appointment created, but the confirmation email could not be sent.";
            }
        }

        private async Task SendAppointmentUpdateNotificationsAsync(Appointment updatedAppointment, DateTime originalDate, string originalMechanicId)
        {
            if (updatedAppointment == null || (originalDate == updatedAppointment.Date && originalMechanicId == updatedAppointment.MechanicId))
            {
                return;
            }

            var oldDateStr = originalDate.ToString("dddd, MMMM dd 'at' h:mm tt");
            var newDateStr = updatedAppointment.Date.ToString("dddd, MMMM dd 'at' h:mm tt");

            var clientMessage = $"Update: Your appointment for {updatedAppointment.Vehicle.LicensePlate} has been rescheduled to {newDateStr}.";
            await _notificationHub.Clients.User(updatedAppointment.ClientId).ReceiveNotification(clientMessage, Url.Action("MyAppointments"), "bi-calendar-event");

            var mechanicMessage = $"Update: Appointment for {updatedAppointment.Vehicle.LicensePlate} has been moved to {newDateStr}.";
            await _notificationHub.Clients.User(updatedAppointment.MechanicId).ReceiveNotification(mechanicMessage, Url.Action("Index", "Dashboard"), "bi-calendar-event");

            if (originalMechanicId != updatedAppointment.MechanicId && !string.IsNullOrEmpty(originalMechanicId))
            {
                var oldMechanicMessage = $"Cancelled: The appointment for {updatedAppointment.Vehicle.LicensePlate} on {oldDateStr} has been reassigned.";
                await _notificationHub.Clients.User(originalMechanicId).ReceiveNotification(oldMechanicMessage, Url.Action("Index", "Dashboard"), "bi-calendar-x");
            }

            try
            {
                var emailViewModel = new Models.Email.AppointmentRescheduledEmailViewModel
                {
                    ClientFirstName = updatedAppointment.Client.FirstName,
                    VehicleDescription = $"{updatedAppointment.Vehicle.CarModel.Brand.Name} {updatedAppointment.Vehicle.CarModel.Name}",
                    LicensePlate = updatedAppointment.Vehicle.LicensePlate,
                    OldAppointmentDate = oldDateStr,
                    NewAppointmentDate = newDateStr
                };
                var emailBody = await _viewRenderer.RenderToStringAsync("/Views/Shared/_EmailTemplates/AppointmentRescheduledEmail.cshtml", emailViewModel);
                _mailHelper.SendEmail(updatedAppointment.Client.Email, "Appointment Rescheduled - FredAuto Workshop", emailBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending appointment update email: {ex.Message}");
                TempData["WarningMessage"] = "Appointment updated, but the confirmation email could not be sent.";
            }
        }

        private async Task SendAppointmentCancellationNotificationsAsync(Appointment appointment)
        {
            var appointmentDateStr = appointment.Date.ToString("dddd, MMMM dd 'at' h:mm tt");
            var mechanicUrl = Url.Action("Index", "Dashboard");

            var mechanicMessage = $"Cancelled: Your appointment for {appointment.Vehicle.LicensePlate} on {appointmentDateStr} has been cancelled.";
            await _notificationHub.Clients.User(appointment.MechanicId).ReceiveNotification(mechanicMessage, mechanicUrl, "bi-calendar-x-fill text-danger");

            var clientMessage = $"Your appointment for {appointmentDateStr} has been cancelled by the workshop. Please contact us to reschedule.";
            await _notificationHub.Clients.User(appointment.ClientId).ReceiveNotification(clientMessage, Url.Action("Index", "Home"), "bi-calendar-x-fill text-danger");

            try
            {
                var emailViewModel = new Models.Email.AppointmentCancelledEmailViewModel
                {
                    ClientFirstName = appointment.Client.FirstName,
                    AppointmentDate = appointmentDateStr,
                    LicensePlate = appointment.Vehicle.LicensePlate
                };
                var emailBody = await _viewRenderer.RenderToStringAsync("/Views/Shared/_EmailTemplates/AppointmentCancelledEmail.cshtml", emailViewModel);
                _mailHelper.SendEmail(appointment.Client.Email, "Appointment Cancellation Notice - FredAuto Workshop", emailBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending cancellation email: {ex.Message}");
            }
        }

        private async Task RepopulateDropdownsForCreateAsync(AppointmentViewModel model)
        {
            model.Clients = await GetClientSelectListAsync();
            model.ServiceTypes = await GetServiceTypeSelectListAsync(model.ServiceTypeId);
            model.Vehicles = !string.IsNullOrEmpty(model.ClientId)
                ? await GetVehicleSelectListAsync(model.ClientId, model.VehicleId)
                : new SelectList(Enumerable.Empty<SelectListItem>());
            model.Mechanics = new SelectList(Enumerable.Empty<SelectListItem>(), "Value", "Text");
        }

        private async Task RepopulateDropdownsForEditAsync(AppointmentViewModel model)
        {
            model.ServiceTypes = await GetServiceTypeSelectListAsync(model.ServiceTypeId);
            model.Mechanics = await GetMechanicSelectListForEditAsync(model.MechanicId);
        }

        private async Task<IEnumerable<SelectListItem>> GetClientSelectListAsync()
        {
            var clients = await _userHelper.GetUsersInRoleAsync("Client");
            var list = clients.Select(u => new SelectListItem { Value = u.Id, Text = $"{u.FullName} ({u.Email})" }).OrderBy(t => t.Text).ToList();
            list.Insert(0, new SelectListItem { Value = "", Text = "Select a client..." });
            return list;
        }

        private async Task<IEnumerable<SelectListItem>> GetVehicleSelectListAsync(string clientId, int? selectedVehicleId = null)
        {
            var vehicles = await _vehicleRepo.GetVehiclesByOwnerIdAsync(clientId);
            return new SelectList(vehicles.OrderBy(v => v.LicensePlate), "Id", "LicensePlate", selectedVehicleId);
        }

        private async Task<IEnumerable<SelectListItem>> GetServiceTypeSelectListAsync(int? selectedId = null)
        {
            var types = await _repairTypeRepo.GetAllAsync();
            var list = types.Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Name }).OrderBy(t => t.Text).ToList();
            list.Insert(0, new SelectListItem { Value = "0", Text = "Select a service type..." });
            if (selectedId.HasValue)
            {
                var selectedItem = list.FirstOrDefault(x => x.Value == selectedId.Value.ToString());
                if (selectedItem != null) selectedItem.Selected = true;
            }
            return list;
        }

        private async Task<IEnumerable<SelectListItem>> GetMechanicSelectListForEditAsync(string currentMechanicId)
        {
            var mechanics = await _userHelper.GetUsersInRoleAsync("Mechanic");
            return new SelectList(mechanics.OrderBy(m => m.FullName), "Id", "FullName", currentMechanicId);
        }

        #endregion

        #region API Methods for AJAX calls

        [HttpGet]
        [Authorize(Roles = "Receptionist")]
        public async Task<JsonResult> GetVehiclesByClient(string clientId)
        {
            if (string.IsNullOrEmpty(clientId)) return Json(new SelectList(Enumerable.Empty<SelectListItem>()));

            var vehicles = await _vehicleRepo.GetVehiclesByOwnerIdAsync(clientId);
            var vehicleList = vehicles.Select(v => new { Value = v.Id, Text = $"{v.LicensePlate} ({v.CarModel.Brand.Name} {v.CarModel.Name})" }).OrderBy(v => v.Text);

            return Json(new SelectList(vehicleList, "Value", "Text"));
        }

        [HttpGet]
        [Authorize(Roles = "Receptionist")]
        public async Task<JsonResult> GetAvailableMechanics(DateTime appointmentDate)
        {
            var mechanics = await _appointmentRepo.GetAvailableMechanicsAsync(appointmentDate);
            var mechanicList = mechanics.Select(u => new SelectListItem { Value = u.Id, Text = u.FullName }).ToList();
            return Json(mechanicList);
        }

        [HttpGet]
        [Authorize(Roles = "Receptionist")]
        public async Task<JsonResult> GetUnavailableDays(int year, int month)
        {
            var unavailableDays = await _appointmentRepo.GetUnavailableDaysAsync(year, month);
            return Json(unavailableDays);
        }

        #endregion
    }
}