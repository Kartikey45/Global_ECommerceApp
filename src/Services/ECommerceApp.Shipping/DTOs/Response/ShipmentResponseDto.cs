namespace ECommerceApp.Shipping.DTOs.Response
{
    public class ShipmentResponseDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string UserId { get; set; }
            = string.Empty;
        public string UserEmail { get; set; }
            = string.Empty;
        public string TrackingNumber { get; set; }
            = string.Empty;
        public string Carrier { get; set; }
            = string.Empty;
        public string Status { get; set; }
            = string.Empty;
        public string DeliveryAddress { get; set; }
            = string.Empty;
        public DateTime EstimatedDelivery { get; set; }
        public DateTime? ShippedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}