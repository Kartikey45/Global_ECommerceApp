using ECommerceApp.Notification.Services.Interfaces;
using ECommerceApp.Notification.Templates;
using ECommerceApp.Shared.Events;
using MassTransit;

namespace ECommerceApp.Notification.Consumers
{
    public class OrderPlacedConsumer
        : IConsumer<OrderPlacedEvent>
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<OrderPlacedConsumer>
            _logger;

        public OrderPlacedConsumer(
            IEmailService emailService,
            ILogger<OrderPlacedConsumer> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        public async Task Consume(
            ConsumeContext<OrderPlacedEvent> context)
        {
            var evt = context.Message;

            _logger.LogInformation(
                "Sending order confirmation email " +
                "to {Email} for Order {OrderId}",
                evt.UserEmail, evt.OrderId);

            // Map event items to template items
            var templateItems = evt.Items.Select(i =>
                new OrderItemTemplate
                {
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    TotalPrice = i.TotalPrice
                }).ToList();

            await _emailService.SendAsync(
                toEmail: evt.UserEmail,
                toName: evt.UserEmail,
                subject: $"Order #{evt.OrderId} Received",
                htmlBody: EmailTemplates.OrderPlacedEmail(
                    evt.UserEmail,
                    evt.OrderId,
                    evt.TotalAmount,
                    evt.ShippingAddress,
                    templateItems));
        }
    }
}