using ECommerceApp.Notification.Services.Interfaces;
using ECommerceApp.Notification.Templates;
using ECommerceApp.Shared.Events;
using MassTransit;

namespace ECommerceApp.Notification.Consumers
{
    public class UserRegisteredConsumer
        : IConsumer<UserRegisteredEvent>
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<UserRegisteredConsumer>
            _logger;

        public UserRegisteredConsumer(
            IEmailService emailService,
            ILogger<UserRegisteredConsumer> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        public async Task Consume(
            ConsumeContext<UserRegisteredEvent> context)
        {
            var evt = context.Message;

            _logger.LogInformation(
                "Sending welcome email to {Email}",
                evt.Email);

            await _emailService.SendAsync(
                toEmail: evt.Email,
                toName: evt.FullName,
                subject: "Welcome to ECommerce App!",
                htmlBody: EmailTemplates
                    .WelcomeEmail(evt.FullName));
        }
    }
}