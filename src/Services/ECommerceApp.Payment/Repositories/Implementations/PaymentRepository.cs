using ECommerceApp.Payment.Data;
using ECommerceApp.Payment.Enums;
using ECommerceApp.Payment.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ECommerceApp.Payment.Repositories.Implementations
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly PaymentDbContext _context;
        private readonly ILogger<PaymentRepository> _logger;

        public PaymentRepository(
            PaymentDbContext context,
            ILogger<PaymentRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ── EF Core: Get by ID ────────────────────────────
        public async Task<Models.Payment?> GetByIdAsync(
            int id)
            => await _context.Payments
                .FirstOrDefaultAsync(p => p.Id == id);

        // ── EF Core: Get by OrderId ───────────────────────
        public async Task<Models.Payment?> GetByOrderIdAsync(
            int orderId)
            => await _context.Payments
                .FirstOrDefaultAsync(
                    p => p.OrderId == orderId);

        // ── EF Core: Get by UserId ────────────────────────
        public async Task<List<Models.Payment>>
            GetByUserIdAsync(string userId)
            => await _context.Payments
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

        // ── Stored Procedure: Save Payment (atomic) ───────
        public async Task<int> SavePaymentAsync(
            Models.Payment payment)
        {
            var orderIdParam = new SqlParameter(
                "@OrderId", payment.OrderId);
            var userIdParam = new SqlParameter(
                "@UserId", payment.UserId);
            var emailParam = new SqlParameter(
                "@UserEmail", payment.UserEmail);
            var amountParam = new SqlParameter(
                "@Amount", payment.Amount);
            var currencyParam = new SqlParameter(
                "@Currency", payment.Currency);
            var statusParam = new SqlParameter(
                "@Status", payment.Status.ToString());
            var txIdParam = new SqlParameter(
                "@TransactionId",
                (object?)payment.TransactionId
                ?? DBNull.Value);
            var failureParam = new SqlParameter(
                "@FailureReason",
                (object?)payment.FailureReason
                ?? DBNull.Value);
            var paymentIdParam = new SqlParameter
            {
                ParameterName = "@PaymentId",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC usp_SavePayment " +
                "@OrderId, @UserId, @UserEmail, " +
                "@Amount, @Currency, @Status, " +
                "@TransactionId, @FailureReason, " +
                "@PaymentId OUTPUT",
                orderIdParam, userIdParam, emailParam,
                amountParam, currencyParam, statusParam,
                txIdParam, failureParam, paymentIdParam);

            return (int)paymentIdParam.Value;
        }

        // ── Stored Procedure: Update Status (atomic) ──────
        public async Task UpdatePaymentStatusAsync(
            int paymentId,
            PaymentStatus status,
            string? transactionId = null,
            string? failureReason = null,
            int? retryCount = null)
        {
            var idParam = new SqlParameter(
                "@PaymentId", paymentId);
            var statusParam = new SqlParameter(
                "@Status", status.ToString());
            var txParam = new SqlParameter(
                "@TransactionId",
                (object?)transactionId ?? DBNull.Value);
            var failParam = new SqlParameter(
                "@FailureReason",
                (object?)failureReason ?? DBNull.Value);
            var retryParam = new SqlParameter(
                "@RetryCount",
                (object?)retryCount ?? DBNull.Value);

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC usp_UpdatePaymentStatus " +
                "@PaymentId, @Status, " +
                "@TransactionId, @FailureReason, " +
                "@RetryCount",
                idParam, statusParam, txParam,
                failParam, retryParam);
        }

        public async Task<bool> ExistsCompletedAsync(
            int orderId)
            => await _context.Payments
                .AnyAsync(p =>
                    p.OrderId == orderId &&
                    p.Status == PaymentStatus.Completed);
    }
}