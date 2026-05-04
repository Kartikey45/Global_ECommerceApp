namespace ECommerceApp.Shared.Events
{
    public class OrderShippedEvent
    {
        public Guid MessageId { get; set; } = Guid.NewGuid();
        public int OrderId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string TrackingNumber { get; set; } = string.Empty;
        public string Carrier { get; set; } = string.Empty;
        public DateTime EstimatedDelivery { get; set; }
        public DateTime ShippedAt { get; set; } = DateTime.UtcNow;
    }
}
