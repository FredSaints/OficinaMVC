using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
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
        private readonly IRepairService _repairService;
        private readonly IPartRepository _partRepository;
        private readonly IUserHelper _userHelper;
        private readonly IHubContext<NotificationHub, INotificationClient> _notificationHub;

        public RepairsController(
            IRepairRepository repairRepository,
            IRepairService repairService,
            IPartRepository partRepository,
            IUserHelper userHelper,
            IHubContext<NotificationHub, INotificationClient> notificationHub)
        {
            _repairRepository = repairRepository;
            _repairService = repairService;
            _partRepository = partRepository;
            _userHelper = userHelper;
            _notificationHub = notificationHub;
        }

        public async Task<IActionResult> Index(string status, string clientName, DateTime? startDate, DateTime? endDate)
        {
            var repairs = await _repairService.GetFilteredRepairsAsync(status, clientName, startDate, endDate);

            ViewData["CurrentStatus"] = status;
            ViewData["CurrentClientName"] = clientName;
            ViewData["CurrentStartDate"] = startDate?.ToString("yyyy-MM-dd");
            ViewData["CurrentEndDate"] = endDate?.ToString("yyyy-MM-dd");

            return View(repairs);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var repair = await _repairRepository.GetByIdWithDetailsAsync(id);
            if (repair == null) return NotFound();

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

        public async Task<IActionResult> Details(int id)
        {
            var repair = await _repairRepository.GetByIdWithDetailsAsync(id);
            if (repair == null) return NotFound();
            return View(repair);
        }

        public async Task<IActionResult> StartRepairFromAppointment(int appointmentId)
        {
            if (appointmentId == 0) return BadRequest();
            try
            {
                var repair = await _repairRepository.CreateRepairFromAppointmentAsync(appointmentId);
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
            var repairPart = await _repairRepository.GetRepairPartByIdAsync(repairPartId);
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
            var completedRepair = await _repairService.CompleteRepairAsync(id);
            if (completedRepair != null)
            {
                TempData["SuccessMessage"] = $"Repair #{completedRepair.Id} has been marked as completed.";
                if (string.IsNullOrEmpty(TempData["WarningMessage"] as string))
                {
                    TempData["WarningMessage"] = "Repair completed, but the confirmation email could not be sent to the client.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "This repair could not be completed. It may already be completed or does not exist.";
            }

            return RedirectToAction(nameof(Index));
        }

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