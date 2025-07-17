using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OficinaMVC.Data;
using OficinaMVC.Helpers;
using OficinaMVC.Models.Communication;
using OficinaMVC.Services;

namespace OficinaMVC.Controllers
{
    [Authorize(Roles = "Admin,Receptionist")]
    public class CommunicationController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IMailHelper _mailHelper;
        private readonly DataContext _context;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public CommunicationController(IUserHelper userHelper,
                                       IMailHelper mailHelper,
                                       DataContext context,
                                       IBackgroundJobClient backgroundJobClient)
        {
            _userHelper = userHelper;
            _mailHelper = mailHelper;
            _context = context;
            _backgroundJobClient = backgroundJobClient;
        }

        public IActionResult Index()
        {
            return View();
        }

        // GET: /Communication/SendAnnouncement
        public IActionResult SendAnnouncement()
        {
            return View(new CommunicationViewModel());
        }

        // POST: /Communication/SendAnnouncement
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendAnnouncement(CommunicationViewModel model, [FromHeader(Name = "X-SignalR-Connection-Id")] string connectionId)
        {
            if (string.IsNullOrEmpty(connectionId))
            {
                // This is a safeguard. The client should always send its connection ID.
                return BadRequest("SignalR connection ID is missing.");
            }

            if (ModelState.IsValid)
            {
                var clients = await _userHelper.GetUsersInRoleAsync("Client");
                var clientEmails = clients.Select(c => c.Email).ToList();

                // Enqueue the background job, passing all necessary data
                _backgroundJobClient.Enqueue<IBulkEmailService>(service =>
                    service.SendAnnouncements(clientEmails, model.Subject, model.Message, connectionId));

                // Return an immediate success response to the AJAX call
                return Ok(new { message = "Email job has been successfully queued." });
            }

            return BadRequest(ModelState);
        }

        // GET: /Communication/BulkCancel
        public async Task<IActionResult> BulkCancel()
        {
            var mechanics = await _userHelper.GetUsersInRoleAsync("Mechanic");
            var model = new BulkCancelViewModel
            {
                Mechanics = new SelectList(mechanics.OrderBy(m => m.FullName), "Id", "FullName")
            };
            return View(model);
        }

        // POST: /Communication/BulkCancelConfirmed
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkCancelConfirmed(BulkCancelViewModel model)
        {
            // THE FIX: Remove the validation error for the dropdown collection property.
            ModelState.Remove("Mechanics");

            if (!ModelState.IsValid)
            {
                // If no mechanic was selected, redisplay the form
                var mechanics = await _userHelper.GetUsersInRoleAsync("Mechanic");
                model.Mechanics = new SelectList(mechanics.OrderBy(m => m.FullName), "Id", "FullName");
                return View("BulkCancel", model);
            }

            // Find all upcoming, confirmed appointments for this mechanic
            var appointmentsToCancel = await _context.Appointments
                .Include(a => a.Client)
                .Where(a => a.MechanicId == model.MechanicId &&
                            a.Date >= DateTime.Today &&
                            a.Status == "Confirmed")
                .ToListAsync();

            if (!appointmentsToCancel.Any())
            {
                TempData["SuccessMessage"] = "No upcoming appointments found for this mechanic to cancel.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var appt in appointmentsToCancel)
            {
                // 1. Send cancellation email to the client
                var subject = $"Important: Your Appointment on {appt.Date:dd-MM-yyyy} has been cancelled";
                var body = $@"<p>Hello {appt.Client.FirstName},</p>
                      <p>Due to unforeseen circumstances, we have had to cancel your upcoming appointment scheduled for {appt.Date:g}.</p>
                      <p>We sincerely apologize for any inconvenience this may cause. Please contact us at your earliest convenience to reschedule.</p>
                      <p>Thank you for your understanding.</p>
                      <p><em>The FredAuto Team</em></p>";
                _mailHelper.SendEmail(appt.Client.Email, subject, body);

                // 2. Update the appointment status to "Cancelled"
                appt.Status = "Cancelled";
            }

            // 3. Save all the status changes to the database
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{appointmentsToCancel.Count} appointment(s) have been successfully cancelled and clients notified.";
            return RedirectToAction(nameof(Index));
        }
    }
}