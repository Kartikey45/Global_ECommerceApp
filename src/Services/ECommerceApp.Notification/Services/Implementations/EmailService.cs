using ECommerceApp.Notification.Services.Interfaces;
using MailKit.Net.Smtp;
using MimeKit;

namespace ECommerceApp.Notification.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IConfiguration config,
            ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendAsync(
            string toEmail,
            string toName,
            string subject,
            string htmlBody)
        {
            try
            {
                var message = new MimeMessage();

                // From
                message.From.Add(new MailboxAddress(
                    _config["EmailSettings:FromName"]
                        ?? "ECommerce App",
                    _config["EmailSettings:FromEmail"]
                        ?? "noreply@ecommerceapp.com"));

                // To
                message.To.Add(
                    new MailboxAddress(toName, toEmail));

                // Subject
                message.Subject = subject;

                // HTML Body
                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = htmlBody
                };
                message.Body = bodyBuilder.ToMessageBody();

                // Send via Mailhog SMTP
                using var client = new SmtpClient();

                await client.ConnectAsync(
                    _config["EmailSettings:SmtpHost"]
                        ?? "localhost",
                    int.Parse(
                        _config["EmailSettings:SmtpPort"]
                        ?? "1025"),
                    false); // No SSL for Mailhog

                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation(
                    "Email sent → {To} | Subject: {Subject}",
                    toEmail, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send email → {To} | " +
                    "Subject: {Subject}",
                    toEmail, subject);

                // Don't throw — email failure should not
                // crash the consumer or cause retries
                // Notification is best-effort
            }
        }
    }
}