using ECommerceApp.Notification.Services.Interfaces;
using ECommerceApp.Notification.Templates;
using ECommerceApp.Shared.Events;
using MassTransit;

namespace ECommerceApp.Notification.Consumers
{
    public class PaymentProcessedConsumer
        : IConsumer<PaymentProcessedEvent>
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<PaymentProcessedConsumer>
            _logger;

        public PaymentProcessedConsumer(
            IEmailService emailService,
            ILogger<PaymentProcessedConsumer> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        public async Task Consume(
            ConsumeContext<PaymentProcessedEvent> context)
        {
            var evt = context.Message;

            if (evt.IsSuccess)
            {
                _logger.LogInformation(
                    "Sending payment confirmed email " +
                    "to {Email} for Order {OrderId}",
                    evt.UserEmail, evt.OrderId);

                await _emailService.SendAsync(
                    toEmail: evt.UserEmail,
                    toName: evt.UserEmail,
                    subject:
                        $"Payment Confirmed — " +
                        $"Order #{evt.OrderId}",
                    htmlBody: EmailTemplates
                        .PaymentConfirmedEmail(
                            evt.UserEmail,
                            evt.OrderId,
                            evt.Amount,
                            evt.TransactionId
                                ?? "N/A"));
            }
            else
            {
                _logger.LogInformation(
                    "Sending payment failed email " +
                    "to {Email} for Order {OrderId}",
                    evt.UserEmail, evt.OrderId);

                await _emailService.SendAsync(
                    toEmail: evt.UserEmail,
                    toName: evt.UserEmail,
                    subject:
                        $"Payment Failed — " +
                        $"Order #{evt.OrderId}",
                    htmlBody: EmailTemplates
                        .PaymentFailedEmail(
                            evt.UserEmail,
                            evt.OrderId,
                            evt.Amount,
                            evt.FailureReason
                                ?? "Unknown error"));
            }
        }
    }
}