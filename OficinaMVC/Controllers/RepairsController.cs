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
    /// <summary>
    /// Controller for managing repairs, including parts, mechanics, and status updates. Accessible by Admins, Receptionists, and Mechanics.
    /// </summary>
    [Authorize(Roles = "Admin,Receptionist,Mechanic")]
    public class RepairsController : Controller
    {
        private readonly IRepairRepository _repairRepository;
        private readonly IRepairService _repairService;
        private readonly IPartRepository _partRepository;
        private readonly IUserHelper _userHelper;
        private readonly IHubContext<NotificationHub, INotificationClient> _notificationHub;

        /// <summary>
        /// Initializes a new instance of the <see cref="RepairsController"/> class.
        /// </summary>
        /// <param name="repairRepository">Repository for repair data access.</param>
        /// <param name="repairService">Service for repair-related business logic.</param>
        /// <param name="partRepository">Repository for part data access.</param>
        /// <param name="userHelper">Helper for user management operations.</param>
        /// <param name="notificationHub">SignalR hub context for notifications.</param>
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

        /// <summary>
        /// Displays a filtered list of repairs based on status, client name, and date range.
        /// </summary>
        /// <param name="status">The repair status to filter by.</param>
        /// <param name="clientName">The client name to filter by.</param>
        /// <param name="startDate">The start date for filtering repairs.</param>
        /// <param name="endDate">The end date for filtering repairs.</param>
        /// <returns>The repairs index view with filtered results.</returns>
        public async Task<IActionResult> Index(string status, string clientName, DateTime? startDate, DateTime? endDate)
        {
            var repairs = await _repairService.GetFilteredRepairsAsync(status, clientName, startDate, endDate);

            ViewData["CurrentStatus"] = status;
            ViewData["CurrentClientName"] = clientName;
            ViewData["CurrentStartDate"] = startDate?.ToString("yyyy-MM-dd");
            ViewData["CurrentEndDate"] = endDate?.ToString("yyyy-MM-dd");

            return View(repairs);
        }

        /// <summary>
        /// Displays the edit form for a repair, including mechanics and parts.
        /// </summary>
        /// <param name="id">The repair ID.</param>
        /// <returns>The edit repair view or not found.</returns>
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
            ViewBag.PartsList = new SelectList(await _partRepository.GetAllAsync(), "Id", "Name");

            return View(repair);
        }

        /// <summary>
        /// Displays the details of a specific repair.
        /// </summary>
        /// <param name="id">The repair ID.</param>
        /// <returns>The repair details view or not found.</returns>
        public async Task<IActionResult> Details(int id)
        {
            var repair = await _repairRepository.GetByIdWithDetailsAsync(id);
            if (repair == null) return NotFound();
            return View(repair);
        }

        /// <summary>
        /// Starts a repair from an appointment and redirects to its details.
        /// </summary>
        /// <param name="appointmentId">The appointment ID to start a repair from.</param>
        /// <returns>Redirects to the repair details or appointment index on error.</returns>
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
        /// <summary>
        /// Adds a part to a repair and sends a low stock notification if needed.
        /// </summary>
        /// <param name="repairId">The repair ID.</param>
        /// <param name="partId">The part ID to add.</param>
        /// <param name="quantity">The quantity of the part to add.</param>
        /// <returns>Redirects to the edit repair view.</returns>
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
        /// <summary>
        /// Removes a part from a repair.
        /// </summary>
        /// <param name="repairPartId">The repair part ID to remove.</param>
        /// <returns>Redirects to the edit repair view.</returns>
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
        /// <summary>
        /// Updates the description and status of a repair.
        /// </summary>
        /// <param name="id">The repair ID.</param>
        /// <param name="description">The new description for the repair.</param>
        /// <param name="status">The new status for the repair.</param>
        /// <returns>Redirects to the repair details view.</returns>
        public async Task<IActionResult> UpdateDetails(int id, string description, string status)
        {
            await _repairRepository.UpdateRepairStatusAndNotesAsync(id, status, description);
            TempData["SuccessMessage"] = "Repair details updated.";
            return RedirectToAction("Details", new { id });
        }


        /// <summary>
        /// Marks a repair as completed and sets a success or warning message.
        /// </summary>
        /// <param name="id">The repair ID.</param>
        /// <returns>Redirects to the repairs index view.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id)
        {
            var completedRepair = await _repairService.CompleteRepairAsync(id);

            if (completedRepair != null)
            {
                TempData["SuccessMessage"] = $"Repair #{completedRepair.Id} has been marked as completed.";
                return RedirectToAction("GenerateFromRepair", "Invoices", new { repairId = completedRepair.Id });
            }
            else
            {
                TempData["ErrorMessage"] = "This repair could not be completed. It may already be completed or does not exist.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Displays the delete confirmation page for a repair.
        /// </summary>
        /// <param name="id">The repair ID.</param>
        /// <returns>The delete confirmation view or not found.</returns>
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
        /// <summary>
        /// Handles repair deletion and returns parts to stock.
        /// </summary>
        /// <param name="id">The repair ID.</param>
        /// <returns>Redirects to the repairs index view.</returns>
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
            if (selectedMechanicIds == null || !selectedMechanicIds.Any())
            {
                TempData["ErrorMessage"] = "Cannot remove all mechanics from a repair. At least one mechanic must be assigned.";
                return RedirectToAction("Edit", new { id = repairId });
            }
            await _repairRepository.UpdateMechanicsForRepairAsync(repairId, selectedMechanicIds);
            TempData["SuccessMessage"] = "Assigned mechanics have been updated.";
            return RedirectToAction("Edit", new { id = repairId });
        }
    }
}