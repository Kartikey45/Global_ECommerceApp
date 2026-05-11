namespace ECommerceApp.Shared.Events
{
    public class PaymentProcessedEvent
    {
        public Guid MessageId { get; set; } = Guid.NewGuid();
        public int OrderId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }
        public decimal Amount { get; set; }
        public string? TransactionId { get; set; }
        public string? FailureReason { get; set; }
        public string? ShippingAddress { get; set; }
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    }
}
