namespace ECommerceApp.Shared.Events
{
    public class OrderCancelledEvent
    {
        public Guid MessageId { get; set; } = Guid.NewGuid();
        public int OrderId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public bool RefundRequired { get; set; }
        public decimal RefundAmount { get; set; }
        public DateTime CancelledAt { get; set; } = DateTime.UtcNow;
    }
}
