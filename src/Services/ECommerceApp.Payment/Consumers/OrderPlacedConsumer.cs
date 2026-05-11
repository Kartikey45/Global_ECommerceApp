using ECommerceApp.Payment.Services.Interfaces;
using ECommerceApp.Shared.Events;
using MassTransit;

namespace ECommerceApp.Payment.Consumers
{
    // ── Triggered when Order Service places an order ──────
    public class OrderPlacedConsumer
        : IConsumer<OrderPlacedEvent>
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<OrderPlacedConsumer> _logger;

        public OrderPlacedConsumer(
            IPaymentService paymentService,
            ILogger<OrderPlacedConsumer> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        public async Task Consume(
            ConsumeContext<OrderPlacedEvent> context)
        {
            var evt = context.Message;

            _logger.LogInformation(
                "OrderPlacedEvent received — " +
                "OrderId: {OrderId}, " +
                "Amount: {Amount}",
                evt.OrderId, evt.TotalAmount);

            // Process payment
            // Polly retry + circuit breaker
            // runs inside ProcessPaymentAsync
            await _paymentService.ProcessPaymentAsync(
                evt.OrderId,
                evt.UserId,
                evt.UserEmail,
                evt.TotalAmount,
                evt.ShippingAddress);
        }
    }

    // ── DLQ Handler — all retries exhausted ───────────────
    public class OrderPlacedFaultConsumer
        : IConsumer<Fault<OrderPlacedEvent>>
    {
        private readonly ILogger<OrderPlacedFaultConsumer>
            _logger;

        public OrderPlacedFaultConsumer(
            ILogger<OrderPlacedFaultConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(
            ConsumeContext<Fault<OrderPlacedEvent>>
                context)
        {
            var evt = context.Message.Message;

            _logger.LogCritical(
                "DEAD LETTER — OrderPlacedEvent " +
                "failed for OrderId: {OrderId}, " +
                "Amount: {Amount}. " +
                "Error: {Error}. " +
                "Manual intervention required.",
                evt.OrderId,
                evt.TotalAmount,
                context.Message.Exceptions
                    .FirstOrDefault()?.Message);

            // TODO in production:
            // → Save to FailedMessages table
            // → Send alert to admin email
            // → Push to monitoring system

            return Task.CompletedTask;
        }
    }
}