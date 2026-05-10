namespace ECommerceApp.Payment.Enums
{
    public enum PaymentStatus
    {
        Processing = 1,  // Payment initiated
        Completed = 2,  // Payment succeeded
        Failed = 3,  // Payment failed
        Refunded = 4   // Payment refunded
    }
}