namespace OficinaMVC.Services
{
    /// <summary>
    /// Provides methods for sending bulk email announcements to multiple recipients.
    /// </summary>
    public interface IBulkEmailService
    {
        /// <summary>
        /// Sends announcement emails to a list of recipients and reports progress via SignalR.
        /// </summary>
        /// <param name="emails">The list of recipient email addresses.</param>
        /// <param name="subject">The subject of the email.</param>
        /// <param name="message">The body of the email message.</param>
        /// <param name="connectionId">The SignalR connection ID for progress updates.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SendAnnouncements(List<string> emails, string subject, string message, string connectionId);
    }
}
