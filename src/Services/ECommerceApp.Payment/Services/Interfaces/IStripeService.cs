namespace ECommerceApp.Payment.Services.Interfaces
{
    public interface IStripeService
    {
        Task<StripeResult> ChargeAsync(
            int orderId,
            decimal amount,
            string currency);

        Task<StripeResult> RefundAsync(
            string transactionId,
            decimal amount);
    }

    public class StripeResult
    {
        public bool IsSuccess { get; set; }
        public string? TransactionId { get; set; }
        public string? ErrorMessage { get; set; }
    }
}