namespace OficinaMVC.Helpers
{
    /// <summary>
    /// Defines methods for sending emails, with or without attachments.
    /// </summary>
    public interface IMailHelper
    {
        /// <summary>
        /// Sends an email to the specified recipient.
        /// </summary>
        /// <param name="to">The recipient's email address.</param>
        /// <param name="subject">The subject of the email.</param>
        /// <param name="body">The body of the email.</param>
        /// <returns>A response indicating the result of the operation.</returns>
        Response SendEmail(string to, string subject, string body);

        /// <summary>
        /// Sends an email with an attachment to the specified recipient.
        /// </summary>
        /// <param name="to">The recipient's email address.</param>
        /// <param name="subject">The subject of the email.</param>
        /// <param name="body">The body of the email.</param>
        /// <param name="attachment">The attachment data as a byte array.</param>
        /// <param name="attachmentName">The name of the attachment file.</param>
        /// <returns>A response indicating the result of the operation.</returns>
        Response SendEmailWithAttachment(string to, string subject, string body, byte[] attachment, string attachmentName);
    }
}
