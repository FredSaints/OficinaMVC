namespace OficinaMVC.Hubs
{
    public interface INotificationClient
    {
        Task ReceiveNotification(string message, string url, string icon);
        Task progressUpdate(double progress, int sent, int total);
        Task progressComplete(string message);
    }
}
