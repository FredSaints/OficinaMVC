using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OficinaMVC.Helpers;
using OficinaMVC.Models.Communication;
using OficinaMVC.Services;

namespace OficinaMVC.Controllers
{
    [Authorize(Roles = "Admin,Receptionist")]
    public class CommunicationController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly ICommunicationService _communicationService;

        public CommunicationController(
            IUserHelper userHelper,
            IBackgroundJobClient backgroundJobClient,
            ICommunicationService communicationService)
        {
            _userHelper = userHelper;
            _backgroundJobClient = backgroundJobClient;
            _communicationService = communicationService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult SendAnnouncement()
        {
            return View(new CommunicationViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendAnnouncement(CommunicationViewModel model, [FromHeader(Name = "X-SignalR-Connection-Id")] string connectionId)
        {
            if (string.IsNullOrEmpty(connectionId))
            {
                return BadRequest("SignalR connection ID is missing.");
            }

            if (ModelState.IsValid)
            {
                var clients = await _userHelper.GetUsersInRoleAsync("Client");
                var clientEmails = clients.Select(c => c.Email!).ToList();

                _backgroundJobClient.Enqueue<IBulkEmailService>(service =>
                    service.SendAnnouncements(clientEmails, model.Subject, model.Message, connectionId));

                return Ok(new { message = "Email job has been successfully queued." });
            }

            return BadRequest(ModelState);
        }

        public async Task<IActionResult> BulkCancel()
        {
            var mechanics = await _userHelper.GetUsersInRoleAsync("Mechanic");
            var model = new BulkCancelViewModel
            {
                Mechanics = new SelectList(mechanics.OrderBy(m => m.FullName), "Id", "FullName")
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkCancelConfirmed(BulkCancelViewModel model, [FromHeader(Name = "X-SignalR-Connection-Id")] string connectionId)
        {
            if (string.IsNullOrEmpty(connectionId))
            {
                return BadRequest(new { message = "SignalR connection ID is missing." });
            }

            ModelState.Remove("Mechanics");
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            _backgroundJobClient.Enqueue<ICommunicationService>(service =>
                service.BulkCancelAppointmentsForMechanicAsync(model.MechanicId, connectionId, baseUrl));

            return Ok(new { message = "Bulk cancellation job has been successfully queued." });
        }
    }
}