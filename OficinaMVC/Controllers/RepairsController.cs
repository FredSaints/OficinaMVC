using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OficinaMVC.Data;
using OficinaMVC.Data.Repositories;
using OficinaMVC.Helpers;
using OficinaMVC.Hubs;
using OficinaMVC.Services;

namespace OficinaMVC.Controllers
{
    [Authorize(Roles = "Admin,Receptionist,Mechanic")]
    public class RepairsController : Controller
    {
        private readonly IRepairRepository _repairRepository;
        private readonly IPartRepository _partRepository;
        private readonly DataContext _context;
        private readonly IUserHelper _userHelper;
        private readonly IMailHelper _mailHelper;
        private readonly IHubContext<NotificationHub, INotificationClient> _notificationHub;
        private readonly IViewRendererService _viewRenderer;

        public RepairsController(IRepairRepository repairRepository,
                                 IPartRepository partRepository,
                                 DataContext dataContext,
                                 IUserHelper userHelper,
                                  IMailHelper mailHelper,
                                 IHubContext<NotificationHub, INotificationClient> notificationHub,
                                  IViewRendererService viewRenderer)
        {
            _repairRepository = repairRepository;
            _partRepository = partRepository;
            _context = dataContext;
            _userHelper = userHelper;
            _mailHelper = mailHelper;
            _notificationHub = notificationHub;
            _viewRenderer = viewRenderer;
        }

        public async Task<IActionResult> Index(string status, string clientName, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Repairs
                .Include(r => r.Vehicle).ThenInclude(v => v.CarModel).ThenInclude(cm => cm.Brand)
                .Include(r => r.Vehicle).ThenInclude(v => v.Owner)
                .AsQueryable();

            var effectiveStatus = status ?? "Ongoing";

            if (effectiveStatus != "All")
            {
                query = query.Where(r => r.Status == effectiveStatus);
            }

            if (!string.IsNullOrEmpty(clientName))
            {
                query = query.Where(r => (r.Vehicle.Owner.FirstName + " " + r.Vehicle.Owner.LastName).Contains(clientName));
            }

            if (startDate.HasValue)
            {
                query = query.Where(r => r.StartDate.Date >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                query = query.Where(r => r.StartDate.Date <= endDate.Value.Date);
            }

            ViewData["CurrentStatus"] = status;
            ViewData["CurrentClientName"] = clientName;
            ViewData["CurrentStartDate"] = startDate?.ToString("yyyy-MM-dd");
            ViewData["CurrentEndDate"] = endDate?.ToString("yyyy-MM-dd");

            var repairs = await query.OrderByDescending(r => r.StartDate).ToListAsync();
            return View(repairs);
        }

        // GET: Repairs/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var repair = await _repairRepository.GetByIdWithDetailsAsync(id);
            if (repair == null)
            {
                return NotFound();
            }

            if (repair.Status == "Completed")
            {
                TempData["ErrorMessage"] = "Cannot edit a completed repair. View details instead.";
                return RedirectToAction("Details", new { id = id });
            }
            var allMechanics = await _userHelper.GetUsersInRoleAsync("Mechanic");
            var currentlySelectedIds = repair.Mechanics.Select(m => m.Id).ToList();

            ViewBag.AllMechanics = new MultiSelectList(allMechanics.OrderBy(u => u.FullName), "Id", "FullName", currentlySelectedIds);

            return View(repair);
        }

        // GET: Repairs/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var repair = await _repairRepository.GetByIdWithDetailsAsync(id);
            if (repair == null)
            {
                return NotFound();
            }
            return View(repair);
        }


        // GET: Repairs/StartRepairFromAppointment/5
        public async Task<IActionResult> StartRepairFromAppointment(int appointmentId)
        {
            if (appointmentId == 0)
            {
                return BadRequest();
            }

            try
            {
                var repair = await _repairRepository.CreateRepairFromAppointmentAsync(appointmentId);
                // Redirect to the Details page for the newly created or existing repair.
                return RedirectToAction("Details", new { id = repair.Id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index", "Appointment");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPart(int repairId, int partId, int quantity)
        {
            try
            {
                var updatedPart = await _repairRepository.AddPartToRepairAsync(repairId, partId, quantity);
                TempData["SuccessMessage"] = "Part added successfully.";

                if (updatedPart != null && updatedPart.StockQuantity <= 5)
                {
                    var message = $"Low Stock Alert: Only {updatedPart.StockQuantity} units of '{updatedPart.Name}' remain.";
                    var url = Url.Action("Edit", "Parts", new { id = updatedPart.Id });
                    var icon = "bi-exclamation-triangle-fill text-danger";

                    await _notificationHub.Clients.Group("Receptionist").ReceiveNotification(message, url, icon);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction("Edit", new { id = repairId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemovePart(int repairPartId)
        {
            var repairPart = await _context.RepairParts
                .AsNoTracking()
                .FirstOrDefaultAsync(rp => rp.Id == repairPartId);

            if (repairPart == null) return NotFound();

            var repairId = repairPart.RepairId;
            await _repairRepository.RemovePartFromRepairAsync(repairPartId);
            TempData["SuccessMessage"] = "Part removed successfully.";

            return RedirectToAction("Edit", new { id = repairId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDetails(int id, string description, string status)
        {
            await _repairRepository.UpdateRepairStatusAndNotesAsync(id, status, description);
            TempData["SuccessMessage"] = "Repair details updated.";
            return RedirectToAction("Details", new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id)
        {
            var repair = await _repairRepository.GetByIdWithDetailsAsync(id);
            if (repair == null)
            {
                return NotFound();
            }

            if (repair.Status == "Ongoing")
            {
                repair.Status = "Completed";
                repair.EndDate = DateTime.Now;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Repair #{repair.Id} has been marked as completed.";

                var clientMessage = $"Good news! The repair on your vehicle ({repair.Vehicle.LicensePlate}) is complete.";
                var clientUrl = Url.Action("History", "Vehicle", new { id = repair.VehicleId });
                var clientIcon = "bi-check-circle-fill text-success";
                await _notificationHub.Clients.User(repair.Vehicle.OwnerId).ReceiveNotification(clientMessage, clientUrl, clientIcon);

                var receptionistMessage = $"Repair #{repair.Id} for {repair.Vehicle.LicensePlate} is complete. An invoice can now be generated.";
                var receptionistUrl = Url.Action("GenerateFromRepair", "Invoices", new { repairId = repair.Id });
                var receptionistIcon = "bi-receipt text-info";
                await _notificationHub.Clients.Group("Receptionist").ReceiveNotification(receptionistMessage, receptionistUrl, receptionistIcon);

                try
                {
                    var emailViewModel = new Models.Email.RepairCompleteEmailViewModel
                    {
                        ClientFirstName = repair.Vehicle.Owner.FirstName,
                        VehicleDescription = $"{repair.Vehicle.CarModel.Brand.Name} {repair.Vehicle.CarModel.Name}",
                        LicensePlate = repair.Vehicle.LicensePlate,
                        RepairId = repair.Id,
                        CompletionDate = repair.EndDate?.ToString("dddd, MMMM dd, yyyy 'at' h:mm tt") ?? "N/A"
                    };

                    var emailBody = await _viewRenderer.RenderToStringAsync("/Views/Shared/_EmailTemplates/RepairCompleteEmail.cshtml", emailViewModel);

                    var subject = $"Your Repair is Complete - FredAuto Workshop";
                    _mailHelper.SendEmail(repair.Vehicle.Owner.Email, subject, emailBody);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending completion email: {ex.Message}");
                    TempData["WarningMessage"] = "Repair completed, but the confirmation email could not be sent to the client.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "This repair has already been completed.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Repairs/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var repair = await _repairRepository.GetByIdWithDetailsAsync(id);
            if (repair == null) return NotFound();

            if (repair.Status == "Completed")
            {
                TempData["ErrorMessage"] = "Cannot cancel a completed repair.";
                return RedirectToAction(nameof(Index));
            }

            return View(repair);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _repairRepository.DeleteRepairAndReturnPartsToStockAsync(id);
            TempData["SuccessMessage"] = "Repair has been cancelled and parts returned to stock.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateMechanics(int repairId, List<string> selectedMechanicIds)
        {
            await _repairRepository.UpdateMechanicsForRepairAsync(repairId, selectedMechanicIds);
            TempData["SuccessMessage"] = "Assigned mechanics have been updated.";
            return RedirectToAction("Edit", new { id = repairId });
        }
    }
}