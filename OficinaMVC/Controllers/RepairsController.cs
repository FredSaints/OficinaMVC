using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OficinaMVC.Data;
using OficinaMVC.Data.Repositories;

namespace OficinaMVC.Controllers
{
    [Authorize(Roles = "Admin,Receptionist,Mechanic")]
    public class RepairsController : Controller
    {
        private readonly IRepairRepository _repairRepository;
        private readonly IPartRepository _partRepository;
        private readonly DataContext _context;

        public RepairsController(IRepairRepository repairRepository, IPartRepository partRepository, DataContext dataContext)
        {
            _repairRepository = repairRepository;
            _partRepository = partRepository;
            _context = dataContext;
        }

        // GET: Repairs
        public async Task<IActionResult> Index()
        {
            var repairs = await _repairRepository.GetAllWithDetailsAsync();
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
                await _repairRepository.AddPartToRepairAsync(repairId, partId, quantity);
                TempData["SuccessMessage"] = "Part added successfully.";
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
            // We can reuse the UpdateRepairStatusAndNotesAsync method, or create a new one.
            // For clarity, let's just update the status here directly.
            var repair = await _context.Repairs.FindAsync(id); // Assuming DataContext is injected
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
    }
}