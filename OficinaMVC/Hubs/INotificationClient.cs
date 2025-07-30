namespace OficinaMVC.Hubs
{
    /// <summary>
    /// Defines client-side methods for receiving notifications and progress updates via SignalR.
    /// </summary>
    public interface INotificationClient
    {
        /// <summary>
        /// Receives a notification message with an associated URL and icon.
        /// </summary>
        /// <param name="message">The notification message.</param>
        /// <param name="url">The URL related to the notification.</param>
        /// <param name="icon">The icon to display with the notification.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ReceiveNotification(string message, string url, string icon);

        /// <summary>
        /// Receives a progress update with the current progress, sent count, and total count.
        /// </summary>
        /// <param name="progress">The progress value (0-100).</param>
        /// <param name="sent">The number of items sent so far.</param>
        /// <param name="total">The total number of items to send.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task progressUpdate(double progress, int sent, int total);

        /// <summary>
        /// Notifies the client that the progress operation is complete with a final message.
        /// </summary>
        /// <param name="message">The completion message.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task progressComplete(string message);
    }
}
