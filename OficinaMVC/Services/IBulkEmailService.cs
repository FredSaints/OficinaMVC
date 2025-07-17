namespace OficinaMVC.Services
{
    public interface IBulkEmailService
    {
        Task SendAnnouncements(List<string> emails, string subject, string message, string connectionId);
    }
}
