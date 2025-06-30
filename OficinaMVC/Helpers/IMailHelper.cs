namespace OficinaMVC.Helpers
{
    public interface IMailHelper
    {
        Response SendEmail(string to, string subject, string body);
        Response SendEmailWithAttachment(string to, string subject, string body, byte[] attachment, string attachmentName);
    }
}
