namespace OficinaMVC.Hubs
{
    public interface INotificationClient
    {
        Task ReceiveNotification(string message, string url, string icon);
    }
}
