namespace ECommerceApp.Shared.Events
{
    public class OrderPlacedEvent
    {
        public Guid MessageId { get; set; } = Guid.NewGuid();
        public int OrderId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public List<OrderItemEvent> Items { get; set; } = new();
        public DateTime PlacedAt { get; set; } = DateTime.UtcNow;
    }

    public class OrderItemEvent
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
