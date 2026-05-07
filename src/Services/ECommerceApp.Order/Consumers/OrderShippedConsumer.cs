using ECommerceApp.Order.Services.Interfaces;
using ECommerceApp.Shared.Events;
using MassTransit;

namespace ECommerceApp.Order.Consumers
{
    // ── Triggered when Shipping Service ships the order ───
    public class OrderShippedConsumer
        : IConsumer<OrderShippedEvent>
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderShippedConsumer>
            _logger;

        public OrderShippedConsumer(
            IOrderService orderService,
            ILogger<OrderShippedConsumer> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        public async Task Consume(
            ConsumeContext<OrderShippedEvent> context)
        {
            var evt = context.Message;

            _logger.LogInformation(
                "Order shipped event received — " +
                "OrderId: {OrderId}, " +
                "Tracking: {Tracking}",
                evt.OrderId, evt.TrackingNumber);

            // Update status + save tracking number
            await _orderService.UpdateShippingAsync(
                evt.OrderId,
                evt.TrackingNumber,
                evt.Carrier);

            _logger.LogInformation(
                "Order {OrderId} status → Shipped. " +
                "Tracking: {Tracking}",
                evt.OrderId, evt.TrackingNumber);
        }
    }
}