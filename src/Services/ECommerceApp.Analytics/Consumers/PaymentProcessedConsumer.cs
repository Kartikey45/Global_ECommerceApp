using ECommerceApp.Analytics.Services.Interfaces;
using ECommerceApp.Shared.Events;
using MassTransit;

namespace ECommerceApp.Analytics.Consumers
{
    // ── Updates revenue data when payment completes ───
    public class PaymentProcessedConsumer
        : IConsumer<PaymentProcessedEvent>
    {
        private readonly IAnalyticsService _service;
        private readonly ILogger<PaymentProcessedConsumer>
            _logger;

        public PaymentProcessedConsumer(
            IAnalyticsService service,
            ILogger<PaymentProcessedConsumer> logger)
        {
            _service = service;
            _logger = logger;
        }

        public async Task Consume(
            ConsumeContext<PaymentProcessedEvent> context)
        {
            var evt = context.Message;

            _logger.LogInformation(
                "Updating payment analytics for " +
                "Order {OrderId}, Success: {Success}",
                evt.OrderId, evt.IsSuccess);

            await _service.UpdatePaymentAsync(
                evt.OrderId,
                evt.IsSuccess,
                evt.IsSuccess ? evt.Amount : null);
        }
    }
}