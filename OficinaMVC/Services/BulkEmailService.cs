using Hangfire;
using Microsoft.AspNetCore.SignalR;
using OficinaMVC.Helpers;
using OficinaMVC.Hubs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OficinaMVC.Services
{
    public class BulkEmailService : IBulkEmailService
    {
        private readonly IMailHelper _mailHelper;
        private readonly IHubContext<NotificationHub> _hubContext;

        public BulkEmailService(IMailHelper mailHelper, IHubContext<NotificationHub> hubContext)
        {
            _mailHelper = mailHelper;
            _hubContext = hubContext;
        }

        [AutomaticRetry(Attempts = 0)] // Don't retry this job if it fails
        public async Task SendAnnouncements(List<string> emails, string subject, string message, string connectionId)
        {
            int totalEmails = emails.Count;
            int sentCount = 0;

            foreach (var email in emails)
            {
                // In a real app, handle potential SendEmail failure
                _mailHelper.SendEmail(email, subject, message);
                sentCount++;

                // Calculate progress and send an update via SignalR
                double progress = ((double)sentCount / totalEmails) * 100;
                await _hubContext.Clients.Client(connectionId).SendAsync("progressUpdate", progress, sentCount, totalEmails);

                // A small delay to be kind to the email server and allow UI to update
                await Task.Delay(100);
            }

            // Send a final completion message
            await _hubContext.Clients.Client(connectionId).SendAsync("progressComplete", $"Successfully sent {sentCount} announcements.");
        }
    }
}