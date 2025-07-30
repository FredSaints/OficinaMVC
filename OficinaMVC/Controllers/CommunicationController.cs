using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OficinaMVC.Helpers;
using OficinaMVC.Models.Communication;
using OficinaMVC.Services;

namespace OficinaMVC.Controllers
{
    /// <summary>
    /// Controller for sending announcements and bulk communications to users. Accessible by Admins and Receptionists.
    /// </summary>
    [Authorize(Roles = "Admin,Receptionist")]
    public class CommunicationController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly ICommunicationService _communicationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationController"/> class.
        /// </summary>
        /// <param name="userHelper">Helper for user management operations.</param>
        /// <param name="backgroundJobClient">Hangfire background job client.</param>
        /// <param name="communicationService">Service for communication-related business logic.</param>
        public CommunicationController(
            IUserHelper userHelper,
            IBackgroundJobClient backgroundJobClient,
            ICommunicationService communicationService)
        {
            _userHelper = userHelper;
            _backgroundJobClient = backgroundJobClient;
            _communicationService = communicationService;
        }

        /// <summary>
        /// Displays the communication dashboard.
        /// </summary>
        /// <returns>The communication index view.</returns>
        // GET: Communication/Index
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Displays the send announcement form.
        /// </summary>
        /// <returns>The send announcement view.</returns>
        // GET: Communication/SendAnnouncement
        public IActionResult SendAnnouncement()
        {
            return View(new CommunicationViewModel());
        }

        /// <summary>
        /// Handles sending announcements to all clients via background job.
        /// </summary>
        /// <param name="model">The communication view model.</param>
        /// <param name="connectionId">SignalR connection ID for real-time feedback.</param>
        /// <returns>Ok if job queued, or bad request on error.</returns>
        // POST: Communication/SendAnnouncement
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

        /// <summary>
        /// Displays the bulk cancel appointments form for mechanics.
        /// </summary>
        /// <returns>The bulk cancel view with mechanics list.</returns>
        // GET: Communication/BulkCancel
        public async Task<IActionResult> BulkCancel()
        {
            var mechanics = await _userHelper.GetUsersInRoleAsync("Mechanic");
            var model = new BulkCancelViewModel
            {
                Mechanics = new SelectList(mechanics.OrderBy(m => m.FullName), "Id", "FullName")
            };
            return View(model);
        }

        /// <summary>
        /// Handles bulk cancellation of appointments for a mechanic via background job.
        /// </summary>
        /// <param name="model">The bulk cancel view model.</param>
        /// <param name="connectionId">SignalR connection ID for real-time feedback.</param>
        /// <returns>Ok if job queued, or bad request on error.</returns>
        // POST: Communication/BulkCancelConfirmed
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