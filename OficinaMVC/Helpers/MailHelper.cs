using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System; // Required for Exception

namespace OficinaMVC.Helpers
{
    public class MailHelper : IMailHelper
    {
        private readonly IConfiguration _configuration;

        public MailHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

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