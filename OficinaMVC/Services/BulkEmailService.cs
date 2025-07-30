using Hangfire;
using Microsoft.AspNetCore.SignalR;
using OficinaMVC.Helpers;
using OficinaMVC.Hubs;

namespace OficinaMVC.Services
{
    /// <inheritdoc cref="IBulkEmailService"/>
    public class BulkEmailService : IBulkEmailService
    {
        private readonly IMailHelper _mailHelper;
        private readonly IHubContext<NotificationHub, INotificationClient> _hubContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="BulkEmailService"/> class.
        /// </summary>
        /// <param name="mailHelper">The mail helper service for sending emails.</param>
        /// <param name="hubContext">The SignalR hub context for sending progress updates.</param>
        public BulkEmailService(IMailHelper mailHelper, IHubContext<NotificationHub, INotificationClient> hubContext)
        {
            _mailHelper = mailHelper;
            _hubContext = hubContext;
        }

        /// <inheritdoc />
        [AutomaticRetry(Attempts = 0)]
        public async Task SendAnnouncements(List<string> emails, string subject, string message, string connectionId)
        {
            if (string.IsNullOrEmpty(connectionId)) return;

            int totalEmails = emails.Count;
            int sentCount = 0;

            foreach (var email in emails)
            {
                _mailHelper.SendEmail(email, subject, message);
                sentCount++;

                double progress = ((double)sentCount / totalEmails) * 100;

                await _hubContext.Clients.Client(connectionId).progressUpdate(progress, sentCount, totalEmails);

                await Task.Delay(100);
            }

            await _hubContext.Clients.Client(connectionId).progressComplete($"Successfully sent {sentCount} announcements.");
        }
    }
}