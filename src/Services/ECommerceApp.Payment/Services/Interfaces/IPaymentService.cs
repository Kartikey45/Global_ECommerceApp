using ECommerceApp.Payment.DTOs.Response;

namespace ECommerceApp.Payment.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentResponseDto?> GetByIdAsync(int id);
        Task<PaymentResponseDto?> GetByOrderIdAsync(
            int orderId);

        Task ProcessPaymentAsync(
            int orderId,
            string userId,
            string userEmail,
            decimal amount);

        Task<PaymentResponseDto?> ProcessRefundAsync(
            int paymentId);
    }
}