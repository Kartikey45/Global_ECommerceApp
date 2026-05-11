using ECommerceApp.Payment.DTOs.Response;
using ECommerceApp.Payment.Enums;
using ECommerceApp.Payment.Models;
using ECommerceApp.Payment.Policies;
using ECommerceApp.Payment.Repositories.Interfaces;
using ECommerceApp.Payment.Services.Interfaces;
using ECommerceApp.Shared.Events;
using MassTransit;
using Polly.CircuitBreaker;

namespace ECommerceApp.Payment.Services.Implementations
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _repo;
        private readonly IStripeService _stripe;
        private readonly IPublishEndpoint _publisher;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            IPaymentRepository repo,
            IStripeService stripe,
            IPublishEndpoint publisher,
            ILogger<PaymentService> logger)
        {
            _repo = repo;
            _stripe = stripe;
            _publisher = publisher;
            _logger = logger;
        }

        public async Task<PaymentResponseDto?> GetByIdAsync(
            int id)
        {
            var payment = await _repo.GetByIdAsync(id);
            return payment == null
                ? null : MapToDto(payment);
        }

        public async Task<PaymentResponseDto?>
            GetByOrderIdAsync(int orderId)
        {
            var payment =
                await _repo.GetByOrderIdAsync(orderId);
            return payment == null
                ? null : MapToDto(payment);
        }

        // ── Core payment processing with Polly ────────────
        public async Task ProcessPaymentAsync(
            int orderId,
            string userId,
            string userEmail,
            decimal amount,
            string shippingAddress)
        {
            // Prevent duplicate payment
            if (await _repo.ExistsCompletedAsync(orderId))
            {
                _logger.LogWarning(
                    "Payment already completed " +
                    "for Order {OrderId}. Skipping.",
                    orderId);
                return;
            }

            // Step 1: Save payment as Processing
            var payment = new Models.Payment
            {
                OrderId = orderId,
                UserId = userId,
                UserEmail = userEmail,
                Amount = amount,
                Currency = "USD",
                Status = PaymentStatus.Processing
            };

            var paymentId =
                await _repo.SavePaymentAsync(payment);
            payment.Id = paymentId;

            _logger.LogInformation(
                "Processing payment for " +
                "Order {OrderId}, Amount: {Amount}",
                orderId, amount);

            // Step 2: Get Polly combined policy
            // (Retry + Circuit Breaker)
            var policy =
                PaymentPollyPolicies
                    .GetCombinedPolicy(_logger);

            try
            {
                // Step 3: Execute Stripe with Polly
                var result = await policy.ExecuteAsync(
                    async () => await _stripe.ChargeAsync(
                        orderId, amount, "USD"));

                // Step 4a: Payment SUCCESS
                await _repo.UpdatePaymentStatusAsync(
                    paymentId,
                    PaymentStatus.Completed,
                    transactionId: result.TransactionId);

                _logger.LogInformation(
                    "Payment SUCCESS — " +
                    "Order: {OrderId}, " +
                    "TxId: {TxId}",
                    orderId, result.TransactionId);

                // Step 5: Publish success event
                await _publisher.Publish(
                    new PaymentProcessedEvent
                    {
                        OrderId = orderId,
                        UserId = userId,
                        UserEmail = userEmail,
                        IsSuccess = true,
                        Amount = amount,
                        TransactionId = result.TransactionId,
                        ShippingAddress = shippingAddress, // ← ADD
                        ProcessedAt = DateTime.UtcNow
                    });
            }
            catch (BrokenCircuitException ex)
            {
                // Circuit is OPEN — stop all attempts
                _logger.LogCritical(
                    "Circuit OPEN — Payment gateway " +
                    "unavailable for Order {OrderId}. " +
                    "Error: {Error}",
                    orderId, ex.Message);

                await HandlePaymentFailure(
                    paymentId, orderId, userId,
                    userEmail, amount,
                    "Payment gateway unavailable. " +
                    "Please try again later.",
                    payment.RetryCount);

                // Re-throw so MassTransit moves to DLQ
                throw;
            }
            catch (Exception ex)
            {
                // All Polly retries exhausted
                _logger.LogError(ex,
                    "Payment FAILED for " +
                    "Order {OrderId} after all retries.",
                    orderId);

                await HandlePaymentFailure(
                    paymentId, orderId, userId,
                    userEmail, amount,
                    ex.Message,
                    payment.RetryCount + 3);

                // Re-throw so MassTransit moves to DLQ
                throw;
            }
        }

        // ── Handle failure — update DB + publish event ────
        private async Task HandlePaymentFailure(
            int paymentId,
            int orderId,
            string userId,
            string userEmail,
            decimal amount,
            string failureReason,
            int retryCount)
        {
            // Update payment status to Failed
            await _repo.UpdatePaymentStatusAsync(
                paymentId,
                PaymentStatus.Failed,
                failureReason: failureReason,
                retryCount: retryCount);

            // Publish failure event to Order + Notification
            await _publisher.Publish(
                new PaymentProcessedEvent
                {
                    OrderId = orderId,
                    UserId = userId,
                    UserEmail = userEmail,
                    IsSuccess = false,
                    Amount = amount,
                    FailureReason = failureReason,
                    ProcessedAt = DateTime.UtcNow
                });
        }

        // ── Refund processing ─────────────────────────────
        public async Task<PaymentResponseDto?>
            ProcessRefundAsync(int paymentId)
        {
            var payment =
                await _repo.GetByIdAsync(paymentId);

            if (payment == null)
                return null;

            if (payment.Status != PaymentStatus.Completed)
                throw new InvalidOperationException(
                    "Only completed payments " +
                    "can be refunded.");

            // Call Stripe mock refund
            var result = await _stripe.RefundAsync(
                payment.TransactionId!,
                payment.Amount);

            if (!result.IsSuccess)
                throw new InvalidOperationException(
                    "Refund failed. " +
                    "Please try again.");

            // Update status to Refunded
            await _repo.UpdatePaymentStatusAsync(
                paymentId,
                PaymentStatus.Refunded,
                transactionId: result.TransactionId);

            _logger.LogInformation(
                "Refund processed for " +
                "Payment {PaymentId}, " +
                "Order {OrderId}",
                paymentId, payment.OrderId);

            // Return updated payment
            var updated =
                await _repo.GetByIdAsync(paymentId);
            return updated == null
                ? null : MapToDto(updated);
        }

        private static PaymentResponseDto MapToDto(
            Models.Payment p) => new()
            {
                Id = p.Id,
                OrderId = p.OrderId,
                UserId = p.UserId,
                UserEmail = p.UserEmail,
                Amount = p.Amount,
                Currency = p.Currency,
                Status = p.Status.ToString(),
                TransactionId = p.TransactionId,
                FailureReason = p.FailureReason,
                RetryCount = p.RetryCount,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            };
    }
}