using ECommerceApp.Shipping.Services.Interfaces;
using ECommerceApp.Shared.Events;
using MassTransit;

namespace ECommerceApp.Shipping.Consumers
{
    // ── Triggered when payment succeeds ───────────────────
    public class PaymentProcessedConsumer
        : IConsumer<PaymentProcessedEvent>
    {
        private readonly IShippingService _shippingService;
        private readonly ILogger<PaymentProcessedConsumer>
            _logger;

        public PaymentProcessedConsumer(
            IShippingService shippingService,
            ILogger<PaymentProcessedConsumer> logger)
        {
            _shippingService = shippingService;
            _logger = logger;
        }

        public async Task Consume(
            ConsumeContext<PaymentProcessedEvent> context)
        {
            var evt = context.Message;

            // Only create shipment if payment succeeded
            if (!evt.IsSuccess)
            {
                _logger.LogInformation(
                    "Payment failed for Order {OrderId}." +
                    " No shipment created.",
                    evt.OrderId);
                return;
            }

            _logger.LogInformation(
                "Payment success received — " +
                "Creating shipment for " +
                "Order {OrderId}",
                evt.OrderId);

            await _shippingService.CreateShipmentAsync(
                evt.OrderId,
                evt.UserId,
                evt.UserEmail,
                evt.ShippingAddress
                    ?? "Address not provided");
        }
    }

    // ── DLQ Handler ───────────────────────────────────────
    public class PaymentProcessedFaultConsumer
        : IConsumer<Fault<PaymentProcessedEvent>>
    {
        private readonly ILogger <PaymentProcessedFaultConsumer> _logger;

        public PaymentProcessedFaultConsumer(
            ILogger<PaymentProcessedFaultConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(
            ConsumeContext<Fault<PaymentProcessedEvent>>
                context)
        {
            _logger.LogCritical(
                "DEAD LETTER — PaymentProcessedEvent " +
                "failed for OrderId: {OrderId}. " +
                "Shipment NOT created. " +
                "Manual intervention required.",
                context.Message.Message.OrderId);

            return Task.CompletedTask;
        }
    }
}