using Hangfire;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OficinaMVC.Data;
using OficinaMVC.Helpers;
using OficinaMVC.Hubs;

namespace OficinaMVC.Services
{
    public class CommunicationService : ICommunicationService
    {
        private readonly DataContext _context;
        private readonly IMailHelper _mailHelper;
        private readonly IHubContext<NotificationHub, INotificationClient> _hubContext;
        private readonly LinkGenerator _linkGenerator;

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

        [AutomaticRetry(Attempts = 0)]
        public async Task<int> BulkCancelAppointmentsForMechanicAsync(string mechanicId, string connectionId, string baseUrl)
        {
            if (string.IsNullOrEmpty(connectionId)) return 0;

            var appointmentsToCancel = await _context.Appointments
                .Include(a => a.Client)
                .Where(a => a.MechanicId == mechanicId &&
                            a.Date >= DateTime.Today &&
                            a.Status == "Pending")
                .ToListAsync();

            if (!appointmentsToCancel.Any())
            {
                await _hubContext.Clients.Client(connectionId).progressComplete("No upcoming appointments found to cancel.");
                return 0;
            }

            int totalCount = appointmentsToCancel.Count;
            int processedCount = 0;

            foreach (var appt in appointmentsToCancel)
            {
                var appointmentDateStr = appt.Date.ToString("dddd, MMMM dd 'at' h:mm tt");

                var clientMessage = $"Your appointment for {appointmentDateStr} has been cancelled by the workshop due to an emergency. Please contact us to reschedule.";

                var relativeUrl = _linkGenerator.GetPathByAction("MyAppointments", "Appointment");
                var clientUrl = new Uri(new Uri(baseUrl), relativeUrl).ToString();

                var icon = "bi-calendar-x-fill text-danger";
                await _hubContext.Clients.User(appt.ClientId).ReceiveNotification(clientMessage, clientUrl, icon);

                var subject = $"Important: Your Appointment on {appt.Date:dd-MM-yyyy} has been cancelled";
                var body = $@"<p>Hello {appt.Client.FirstName},</p>
                      <p>Due to unforeseen circumstances, we have had to cancel your upcoming appointment scheduled for {appt.Date:g}.</p>
                      <p>We sincerely apologize for any inconvenience this may cause. Please contact us at your earliest convenience to reschedule.</p>
                      <p>Thank you for your understanding.</p>
                      <p><em>The FredAuto Team</em></p>";
                _mailHelper.SendEmail(appt.Client.Email, subject, body);

                appt.Status = "Cancelled";
                processedCount++;

                double progress = ((double)processedCount / totalCount) * 100;
                await _hubContext.Clients.Client(connectionId).progressUpdate(progress, processedCount, totalCount);
                await Task.Delay(100);
            }

            await _context.SaveChangesAsync();

            await _hubContext.Clients.Client(connectionId).progressComplete($"Successfully cancelled {processedCount} appointment(s).");

            return processedCount;
        }
    }
}