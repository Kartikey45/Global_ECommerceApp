using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace ECommerceApp.Payment.Policies
{
    public static class PaymentPollyPolicies
    {
        // ── Retry Policy ──────────────────────────────────
        // Retries 3 times with increasing wait times
        // 1st retry: wait 2 seconds
        // 2nd retry: wait 4 seconds
        // 3rd retry: wait 8 seconds
        public static AsyncRetryPolicy GetRetryPolicy(
            ILogger logger)
        {
            return Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: attempt =>
                        TimeSpan.FromSeconds(
                            Math.Pow(2, attempt)),
                    onRetry: (exception, timeSpan, attempt,
                        context) =>
                    {
                        logger.LogWarning(
                            "Payment attempt {Attempt} failed. " +
                            "Waiting {Seconds}s before retry. " +
                            "Error: {Error}",
                            attempt,
                            timeSpan.TotalSeconds,
                            exception.Message);
                    });
        }

        // ── Circuit Breaker Policy ─────────────────────────
        // Opens circuit after 3 consecutive failures
        // Stays open for 30 seconds
        // Then allows 1 test request (half-open)
        public static AsyncCircuitBreakerPolicy
            GetCircuitBreakerPolicy(ILogger logger)
        {
            return Policy
                .Handle<Exception>()
                .CircuitBreakerAsync(
                    exceptionsAllowedBeforeBreaking: 3,
                    durationOfBreak: TimeSpan.FromSeconds(30),
                    onBreak: (exception, duration) =>
                    {
                        logger.LogCritical(
                            "Circuit OPEN — Payment gateway " +
                            "is unavailable. Will retry in " +
                            "{Seconds}s. Error: {Error}",
                            duration.TotalSeconds,
                            exception.Message);
                    },
                    onReset: () =>
                    {
                        logger.LogInformation(
                            "Circuit CLOSED — Payment gateway " +
                            "is back online.");
                    },
                    onHalfOpen: () =>
                    {
                        logger.LogInformation(
                            "Circuit HALF-OPEN — Testing " +
                            "payment gateway...");
                    });
        }

        // ── Combined Policy ────────────────────────────────
        // Retry wraps Circuit Breaker
        // Circuit Breaker stops retrying if too many fail
        public static IAsyncPolicy GetCombinedPolicy(
            ILogger logger)
        {
            return Policy.WrapAsync(
                GetRetryPolicy(logger),
                GetCircuitBreakerPolicy(logger));
        }
    }
}