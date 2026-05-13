using ECommerceApp.Notification.Services.Interfaces;
using ECommerceApp.Notification.Templates;
using ECommerceApp.Shared.Events;
using MassTransit;

namespace ECommerceApp.Notification.Consumers
{
    public class OrderShippedConsumer
        : IConsumer<OrderShippedEvent>
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<OrderShippedConsumer>
            _logger;

        public OrderShippedConsumer(
            IEmailService emailService,
            ILogger<OrderShippedConsumer> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        public async Task Consume(
            ConsumeContext<OrderShippedEvent> context)
        {
            var evt = context.Message;

            _logger.LogInformation(
                "Sending shipment email to {Email} " +
                "for Order {OrderId}",
                evt.UserEmail, evt.OrderId);

            await _emailService.SendAsync(
                toEmail: evt.UserEmail,
                toName: evt.UserEmail,
                subject:
                    $"Your Order #{evt.OrderId} " +
                    $"Has Shipped!",
                htmlBody: EmailTemplates.OrderShippedEmail(
                    evt.UserEmail,
                    evt.OrderId,
                    evt.TrackingNumber,
                    evt.Carrier,
                    evt.EstimatedDelivery));
        }
    }
}