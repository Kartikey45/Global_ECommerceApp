namespace ECommerceApp.Payment.DTOs.Response
{
    public class PaymentResponseDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string UserId { get; set; }
            = string.Empty;
        public string UserEmail { get; set; }
            = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; }
            = string.Empty;
        public string Status { get; set; }
            = string.Empty;
        public string? TransactionId { get; set; }
        public string? FailureReason { get; set; }
        public int RetryCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}