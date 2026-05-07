using ECommerceApp.Order.Enums;
using ECommerceApp.Order.Services.Interfaces;
using ECommerceApp.Shared.Events;
using MassTransit;

namespace ECommerceApp.Order.Consumers
{
    // ── Triggered when Payment Service processes payment ──
    public class PaymentProcessedConsumer
        : IConsumer<PaymentProcessedEvent>
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<PaymentProcessedConsumer>
            _logger;

        public PaymentProcessedConsumer(
            IOrderService orderService,
            ILogger<PaymentProcessedConsumer> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        public async Task Consume(
            ConsumeContext<PaymentProcessedEvent> context)
        {
            var evt = context.Message;

            _logger.LogInformation(
                "Payment result received — " +
                "OrderId: {OrderId}, Success: {Success}",
                evt.OrderId, evt.IsSuccess);

            if (evt.IsSuccess)
            {
                // Payment succeeded → Confirmed
                await _orderService.UpdateStatusAsync(
                    evt.OrderId,
                    OrderStatus.Confirmed);

                _logger.LogInformation(
                    "Order {OrderId} status → Confirmed",
                    evt.OrderId);
            }
            else
            {
                // Payment failed → PaymentFailed
                await _orderService.UpdateStatusAsync(
                    evt.OrderId,
                    OrderStatus.PaymentFailed);

                _logger.LogWarning(
                    "Order {OrderId} status → " +
                    "PaymentFailed. Reason: {Reason}",
                    evt.OrderId, evt.FailureReason);
            }
        }
    }

    // ── DLQ Handler — runs when all retries exhausted ────
    public class PaymentProcessedFaultConsumer
        : IConsumer<Fault<PaymentProcessedEvent>>
    {
        private readonly ILogger
            <PaymentProcessedFaultConsumer> _logger;

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
                "Error: {Error}",
                context.Message.Message.OrderId,
                context.Message.Exceptions
                    .FirstOrDefault()?.Message);

            return Task.CompletedTask;
        }
    }
}