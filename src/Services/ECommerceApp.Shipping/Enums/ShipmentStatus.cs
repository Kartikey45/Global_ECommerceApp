namespace ECommerceApp.Shipping.Enums
{
    public enum ShipmentStatus
    {
        Processing = 1, // Shipment created
        Dispatched = 2, // Handed to courier
        InTransit = 3, // On the way
        OutForDelivery = 4, // With delivery driver
        Delivered = 5, // Received by customer
        Failed = 6  // Delivery failed
    }
}