using ECommerceApp.Payment.Enums;

namespace ECommerceApp.Payment.Repositories.Interfaces
{
    public interface IPaymentRepository
    {
        Task<Models.Payment?> GetByIdAsync(int id);
        Task<Models.Payment?> GetByOrderIdAsync(int orderId);
        Task<List<Models.Payment>> GetByUserIdAsync(
            string userId);

        // Stored Procedure: atomic save
        Task<int> SavePaymentAsync(Models.Payment payment);

        // Stored Procedure: atomic status update
        Task UpdatePaymentStatusAsync(
            int paymentId,
            PaymentStatus status,
            string? transactionId = null,
            string? failureReason = null,
            int? retryCount = null);

        Task<bool> ExistsCompletedAsync(int orderId);
    }
}