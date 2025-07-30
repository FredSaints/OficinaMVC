using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace OficinaMVC.Helpers
{
    /// <summary>
    /// Provides methods for sending emails, with or without attachments, using SMTP.
    /// </summary>
    public class MailHelper : IMailHelper
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="MailHelper"/> class.
        /// </summary>
        /// <param name="configuration">The application configuration for email settings.</param>
        public MailHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <inheritdoc />
        public Response SendEmail(string to, string subject, string body)
        {
            // Get the configuration settings correctly
            var nameFrom = _configuration["Mail:NameFrom"];
            var from = _configuration["Mail:From"];
            var smtp = _configuration["Mail:Smtp"];
            var portString = _configuration["Mail:Port"];
            var password = _configuration["Mail:Password"];

            // It's good practice to check if the settings were found
            if (string.IsNullOrEmpty(smtp) || string.IsNullOrEmpty(from))
            {
                // This will help you debug config issues in the future
                throw new ArgumentException("Email settings (SMTP, From) are missing in appsettings.json");
            }

            if (!int.TryParse(portString, out int port))
            {
                throw new FormatException($"Mail:Port configuration ('{portString}') is not a valid integer.");
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(nameFrom, from));
            message.To.Add(new MailboxAddress(to, to));
            message.Subject = subject;

            var bodybuilder = new BodyBuilder()
            {
                HtmlBody = body
            };
            message.Body = bodybuilder.ToMessageBody();

            try
            {
                using (var client = new SmtpClient())
                {
                    SecureSocketOptions options = SecureSocketOptions.Auto;
                    if (port == 587) options = SecureSocketOptions.StartTls;
                    else if (port == 465) options = SecureSocketOptions.SslOnConnect;

                    client.Connect(smtp, port, options);
                    if (!string.IsNullOrEmpty(password)) client.Authenticate(from, password);
                    client.Send(message);
                    client.Disconnect(true);
                }
            }
            catch (Exception ex)
            {
                return new Response { IsSuccess = false, Message = ex.ToString() };
            }

            return new Response { IsSuccess = true };
        }

        /// <inheritdoc />
        public Response SendEmailWithAttachment(string to, string subject, string body, byte[] attachmentData, string attachmentName)
        {
            var nameFrom = _configuration["Mail:Namefrom"];
            var from = _configuration["Mail:From"];
            var smtp = _configuration["Mail:Smtp"];
            var portString = _configuration["Mail:Port"];
            var password = _configuration["Mail:Password"];

            if (!int.TryParse(portString, out int port))
            {
                throw new FormatException($"Mail:Port configuration ('{portString}') is not a valid integer.");
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(nameFrom, from));
            message.To.Add(new MailboxAddress(to, to));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = body
            };

            if (attachmentData != null && attachmentData.Length > 0)
            {
                bodyBuilder.Attachments.Add(attachmentName, attachmentData, ContentType.Parse("application/pdf"));
            }

            message.Body = bodyBuilder.ToMessageBody();

            try
            {
                using (var client = new SmtpClient())
                {
                    SecureSocketOptions options = SecureSocketOptions.Auto;
                    if (port == 587)
                    {
                        options = SecureSocketOptions.StartTls;
                    }
                    else if (port == 465)
                    {
                        options = SecureSocketOptions.SslOnConnect;
                    }

                    client.Connect(smtp, port, options);

                    if (!string.IsNullOrEmpty(password))
                    {
                        client.Authenticate(from, password);
                    }

                    client.Send(message);
                    client.Disconnect(true);
                }
            }
            catch (Exception ex)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = ex.ToString()
                };
            }

            return new Response
            {
                IsSuccess = true
            };
        }
    }
}