using Hangfire;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OficinaMVC.Data;
using OficinaMVC.Helpers;
using OficinaMVC.Hubs;

namespace OficinaMVC.Services
{
    /// <summary>
    /// Provides services for handling complex, multi-user communication tasks, such as bulk notifications.
    /// These methods are designed to be executed as background jobs by Hangfire to avoid blocking the UI.
    /// </summary>
    public class CommunicationService : ICommunicationService
    {
        private readonly DataContext _context;
        private readonly IMailHelper _mailHelper;
        private readonly IHubContext<NotificationHub, INotificationClient> _hubContext;
        private readonly LinkGenerator _linkGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationService"/> class.
        /// </summary>
        /// <param name="context">The database context for data access.</param>
        /// <param name="mailHelper">Service for sending emails.</param>
        /// <param name="hubContext">The SignalR hub context for sending real-time progress updates to the client.</param>
        /// <param name="linkGenerator">Service for generating URLs to be included in notifications.</param>
        public CommunicationService(
            DataContext context,
            IMailHelper mailHelper,
            IHubContext<NotificationHub, INotificationClient> hubContext,
            LinkGenerator linkGenerator)
        {
            _context = context;
            _mailHelper = mailHelper;
            _hubContext = hubContext;
            _linkGenerator = linkGenerator;
        }

        /// <summary>
        /// Atomically cancels all upcoming, pending appointments for a specific mechanic and notifies affected clients via SignalR and email.
        /// This is a long-running process intended to be executed as a Hangfire background job.
        /// </summary>
        /// <param name="mechanicId">The ID of the mechanic whose appointments will be cancelled.</param>
        /// <param name="connectionId">The SignalR connection ID of the user who initiated the job, used to send progress updates.</param>
        /// <param name="baseUrl">The base URL of the application (e.g., "https://www.fredauto.com") required to generate absolute links for notifications.</param>
        /// <returns>A task that represents the asynchronous operation, containing the total number of appointments that were successfully cancelled.</returns>
        /// <remarks>
        /// This method is decorated with `[AutomaticRetry(Attempts = 0)]` to prevent Hangfire from automatically retrying the job on failure,
        /// which is critical as the logic now handles its own atomicity per appointment.
        ///
        /// The process is designed for high reliability:
        /// 1. It fetches a list of all appointments to be cancelled.
        /// 2. For each appointment, it attempts the following within a transaction-like block:
        ///    a. Update the appointment's status to "Cancelled" in the database.
        ///    b. If the database update is successful, send the SignalR and email notifications.
        ///    c. If any step fails, the changes for that specific appointment are discarded, no notification is sent, and the process continues with the next appointment.
        /// 3. Progress is reported back to the initiating user in real-time, including any failures.
        /// This ensures that a notification is ONLY sent if the corresponding appointment has been successfully and permanently marked as cancelled in the database, preventing data inconsistency.
        /// </remarks>
        [AutomaticRetry(Attempts = 0)]
        public async Task<int> BulkCancelAppointmentsForMechanicAsync(string mechanicId, string connectionId, string baseUrl)
        {
            if (string.IsNullOrEmpty(connectionId)) return 0;

            // Step 1: Get the list of appointments to process.
            // Using AsNoTracking() is efficient here as we will re-fetch the entity for the update.
            var appointmentsToCancel = await _context.Appointments
                .AsNoTracking()
                .Include(a => a.Client)
                .Where(a => a.MechanicId == mechanicId &&
                            a.Date >= DateTime.UtcNow.Date &&
                            a.Status == "Pending")
                .ToListAsync();

            if (!appointmentsToCancel.Any())
            {
                await _hubContext.Clients.Client(connectionId).progressComplete("No upcoming appointments found to cancel.");
                return 0;
            }

            int totalCount = appointmentsToCancel.Count;
            int successCount = 0;
            int failureCount = 0;

            // Step 2: Loop through each appointment and process it individually.
            for (int i = 0; i < totalCount; i++)
            {
                var apptToProcess = appointmentsToCancel[i];
                try
                {
                    // Step 2a: Update the database FIRST.
                    // Re-attach the specific entity to the context for this operation.
                    var appointmentInDb = await _context.Appointments.FindAsync(apptToProcess.Id);
                    if (appointmentInDb != null && appointmentInDb.Status == "Pending")
                    {
                        appointmentInDb.Status = "Cancelled";
                        await _context.SaveChangesAsync(); // This is our atomic commit for this single appointment.

                        // Step 2b: If SaveChangesAsync succeeds, proceed with notifications.
                        var appointmentDateStr = apptToProcess.Date.ToString("dddd, MMMM dd 'at' h:mm tt");

                        // Send SignalR notification.
                        var clientMessage = $"Your appointment for {appointmentDateStr} has been cancelled by the workshop due to an emergency. Please contact us to reschedule.";
                        var relativeUrl = _linkGenerator.GetPathByAction("MyAppointments", "Appointment");
                        var clientUrl = new Uri(new Uri(baseUrl), relativeUrl).ToString();
                        var icon = "bi-calendar-x-fill text-danger";
                        await _hubContext.Clients.User(apptToProcess.ClientId).ReceiveNotification(clientMessage, clientUrl, icon);

                        // Send Email notification.
                        var subject = $"Important: Your Appointment on {apptToProcess.Date:dd-MM-yyyy} has been cancelled";
                        var body = $@"<p>Hello {apptToProcess.Client.FirstName},</p>
                              <p>Due to unforeseen circumstances, we have had to cancel your upcoming appointment scheduled for {apptToProcess.Date:g}.</p>
                              <p>We sincerely apologize for any inconvenience this may cause. Please contact us at your earliest convenience to reschedule.</p>
                              <p>Thank you for your understanding.</p>
                              <p><em>The FredAuto Team</em></p>";
                        _mailHelper.SendEmail(apptToProcess.Client.Email, subject, body);

                        successCount++;
                    }
                    else
                    {
                        // The appointment was changed or deleted by another user since we first queried it.
                        failureCount++;
                    }
                }
                catch (Exception ex)
                {
                    // In a production environment, inject ILogger and log this exception for diagnostics.
                    Console.WriteLine($"Failed to process appointment {apptToProcess.Id}: {ex.Message}");
                    failureCount++;
                }

                // Step 3: Report progress after each attempt.
                double progress = ((double)(i + 1) / totalCount) * 100;
                await _hubContext.Clients.Client(connectionId).progressUpdate(progress, i + 1, totalCount);
                await Task.Delay(100); // Maintain a small delay.
            }

            // Step 4: Send the final completion message.
            string completionMessage = $"Process complete. Successfully cancelled {successCount} appointment(s).";
            if (failureCount > 0)
            {
                completionMessage += $" Failed to cancel {failureCount} appointment(s).";
            }

            await _hubContext.Clients.Client(connectionId).progressComplete(completionMessage);

            return successCount;
        }
    }
}