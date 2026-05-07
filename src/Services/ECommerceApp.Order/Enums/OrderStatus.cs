namespace ECommerceApp.Order.Enums
{
    public enum OrderStatus
    {
        Pending = 1,  // Order created, awaiting payment
        PaymentProcessing = 2,  // Payment Svc picked up event
        Confirmed = 3,  // Payment succeeded
        PaymentFailed = 4,  // Payment failed
        Shipped = 5,  // Shipping Svc dispatched
        Delivered = 6,  // Delivered to customer
        Cancelled = 7   // Cancelled by user/admin
    }
}