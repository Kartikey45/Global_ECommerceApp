using ECommerceApp.Notification.Services.Interfaces;
using ECommerceApp.Notification.Templates;
using ECommerceApp.Shared.Events;
using MassTransit;

namespace ECommerceApp.Notification.Consumers
{
    public class OrderCancelledConsumer
        : IConsumer<OrderCancelledEvent>
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<OrderCancelledConsumer>
            _logger;

        public OrderCancelledConsumer(
            IEmailService emailService,
            ILogger<OrderCancelledConsumer> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        public async Task Consume(
            ConsumeContext<OrderCancelledEvent> context)
        {
            var evt = context.Message;

            _logger.LogInformation(
                "Sending cancellation email to {Email} " +
                "for Order {OrderId}",
                evt.UserEmail, evt.OrderId);

            await _emailService.SendAsync(
                toEmail: evt.UserEmail,
                toName: evt.UserEmail,
                subject:
                    $"Order #{evt.OrderId} Cancelled",
                htmlBody: EmailTemplates
                    .OrderCancelledEmail(
                        evt.UserEmail,
                        evt.OrderId,
                        evt.Reason,
                        evt.RefundRequired,
                        evt.RefundAmount));
        }
    }
}