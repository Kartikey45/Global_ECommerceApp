using ECommerceApp.Payment.Services.Interfaces;

namespace ECommerceApp.Payment.Services.Implementations
{
    /// <summary>
    /// Mock Stripe service for local development.
    /// Simulates real Stripe API behavior.
    /// Replace with real Stripe SDK in production.
    /// </summary>
    public class StripeService : IStripeService
    {
        private readonly ILogger<StripeService> _logger;
        private readonly IConfiguration _config;

        public StripeService(
            ILogger<StripeService> logger,
            IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public async Task<StripeResult> ChargeAsync(
            int orderId,
            decimal amount,
            string currency)
        {
            // Simulate network delay (real API call)
            await Task.Delay(
                TimeSpan.FromMilliseconds(
                    Random.Shared.Next(100, 500)));

            // Simulate 90% success rate in dev
            var successRate = _config
                .GetValue<int>(
                    "StripeSettings:MockSuccessRate",
                    90);

            var isSuccess =
                Random.Shared.Next(1, 101) <= successRate;

            if (!isSuccess)
            {
                // Simulate different failure reasons
                var failures = new[]
                {
                    "Card declined.",
                    "Insufficient funds.",
                    "Card expired.",
                    "Security code incorrect."
                };

                var reason = failures[
                    Random.Shared.Next(failures.Length)];

                _logger.LogWarning(
                    "Stripe MOCK: Payment failed " +
                    "for Order {OrderId}. Reason: {Reason}",
                    orderId, reason);

                throw new Exception(reason);
            }

            // Generate fake transaction ID
            var transactionId =
                $"STRIPE-{DateTime.UtcNow:yyyyMMddHHmmss}" +
                $"-{Random.Shared.Next(100000, 999999)}";

            _logger.LogInformation(
                "Stripe MOCK: Payment success " +
                "for Order {OrderId}. " +
                "TransactionId: {TxId}",
                orderId, transactionId);

            return new StripeResult
            {
                IsSuccess = true,
                TransactionId = transactionId
            };
        }

        public async Task<StripeResult> RefundAsync(
            string transactionId,
            decimal amount)
        {
            await Task.Delay(
                TimeSpan.FromMilliseconds(
                    Random.Shared.Next(100, 300)));

            var refundId =
                $"REFUND-{DateTime.UtcNow:yyyyMMddHHmmss}" +
                $"-{Random.Shared.Next(100000, 999999)}";

            _logger.LogInformation(
                "Stripe MOCK: Refund processed. " +
                "Original: {TxId}, " +
                "RefundId: {RefundId}, " +
                "Amount: {Amount}",
                transactionId, refundId, amount);

            return new StripeResult
            {
                IsSuccess = true,
                TransactionId = refundId
            };
        }
    }
}