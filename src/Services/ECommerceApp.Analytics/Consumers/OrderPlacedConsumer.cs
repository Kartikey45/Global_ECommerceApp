using ECommerceApp.Analytics.Services.Interfaces;
using ECommerceApp.Shared.Events;
using MassTransit;

namespace ECommerceApp.Analytics.Consumers
{
    // ── Logs order data when order is placed ──────────
    public class OrderPlacedConsumer
        : IConsumer<OrderPlacedEvent>
    {
        private readonly IAnalyticsService _service;
        private readonly ILogger<OrderPlacedConsumer>
            _logger;

        public OrderPlacedConsumer(
            IAnalyticsService service,
            ILogger<OrderPlacedConsumer> logger)
        {
            _service = service;
            _logger = logger;
        }

        public async Task Consume(
            ConsumeContext<OrderPlacedEvent> context)
        {
            var evt = context.Message;

            _logger.LogInformation(
                "Logging analytics for " +
                "Order {OrderId}",
                evt.OrderId);

            await _service.SaveOrderAsync(
                evt.OrderId,
                evt.UserId,
                evt.UserEmail,
                evt.TotalAmount,
                evt.Items.Count);
        }
    }
}