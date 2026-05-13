using ECommerceApp.Analytics.Data;
using ECommerceApp.Analytics.DTOs.Response;
using ECommerceApp.Analytics.Models;
using ECommerceApp.Analytics.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ECommerceApp.Analytics.Repositories
    .Implementations
{
    public class AnalyticsRepository : IAnalyticsRepository
    {
        private readonly AnalyticsDbContext _context;
        private readonly ILogger<AnalyticsRepository>
            _logger;

        public AnalyticsRepository(
            AnalyticsDbContext context,
            ILogger<AnalyticsRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ── EF Core: Save click event ─────────────────
        public async Task SaveClickEventAsync(
            ClickEvent clickEvent)
        {
            _context.ClickEvents.Add(clickEvent);
            await _context.SaveChangesAsync();
        }

        // ── SP: Save order analytic ───────────────────
        public async Task SaveOrderAnalyticAsync(
            OrderAnalytic analytic)
        {
            await _context.Database.ExecuteSqlRawAsync(
                "EXEC usp_SaveOrderAnalytic " +
                "@OrderId, @UserId, @UserEmail, " +
                "@TotalAmount, @ItemCount, @Status",
                new SqlParameter("@OrderId",
                    analytic.OrderId),
                new SqlParameter("@UserId",
                    analytic.UserId),
                new SqlParameter("@UserEmail",
                    analytic.UserEmail),
                new SqlParameter("@TotalAmount",
                    analytic.TotalAmount),
                new SqlParameter("@ItemCount",
                    analytic.ItemCount),
                new SqlParameter("@Status",
                    analytic.Status));
        }

        // ── SP: Update payment result ─────────────────
        public async Task UpdatePaymentResultAsync(
            int orderId,
            bool paymentSuccess,
            decimal? revenueAmount)
        {
            await _context.Database.ExecuteSqlRawAsync(
                "EXEC usp_UpdateOrderAnalyticPayment " +
                "@OrderId, @PaymentSuccess, " +
                "@RevenueAmount",
                new SqlParameter("@OrderId", orderId),
                new SqlParameter("@PaymentSuccess",
                    paymentSuccess),
                new SqlParameter("@RevenueAmount",
                    (object?)revenueAmount
                    ?? DBNull.Value));
        }

        // ── Raw SQL: Order Summary ────────────────────
        public async Task<OrderSummaryDto>
            GetOrderSummaryAsync(
            DateTime? startDate, DateTime? endDate)
        {
            var start = startDate
                ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            var data = await _context.OrderAnalytics
                .Where(o =>
                    o.CreatedAt >= start &&
                    o.CreatedAt <= end)
                .ToListAsync();

            var total = data.Count;
            var success = data.Count(o => o.PaymentSuccess);
            var failed = total - success;
            var revenue = data
                .Where(o => o.PaymentSuccess)
                .Sum(o => o.RevenueAmount ?? 0);

            return new OrderSummaryDto
            {
                TotalOrders = total,
                TotalRevenue = revenue,
                AverageOrderValue = total > 0
                    ? revenue / total : 0,
                SuccessfulPayments = success,
                FailedPayments = failed,
                PaymentSuccessRate = total > 0
                    ? Math.Round(
                        (double)success / total * 100, 2)
                    : 0
            };
        }

        // ── Raw SQL: Daily Revenue ────────────────────
        public async Task<List<DailyRevenueDto>>
            GetDailyRevenueAsync(int days)
        {
            var startDate =
                DateTime.UtcNow.AddDays(-days);

            // Raw SQL for date grouping
            // EF Core LINQ handles date truncation
            // inconsistently across databases
            var result = await _context.Database
                .SqlQueryRaw<DailyRevenueDto>(@"
                    SELECT
                        CAST(CreatedAt AS DATE) AS Date,
                        COUNT(*) AS TotalOrders,
                        ISNULL(SUM(RevenueAmount), 0)
                            AS TotalRevenue,
                        CASE WHEN COUNT(*) > 0
                             THEN ISNULL(SUM(RevenueAmount), 0)
                                  / COUNT(*)
                             ELSE 0
                        END AS AverageOrderValue
                    FROM OrderAnalytics
                    WHERE CreatedAt >= @StartDate
                      AND PaymentSuccess = 1
                    GROUP BY CAST(CreatedAt AS DATE)
                    ORDER BY CAST(CreatedAt AS DATE) DESC",
                    new SqlParameter(
                        "@StartDate", startDate))
                .ToListAsync();

            return result;
        }

        // ── Raw SQL: Top Products by interactions ─────
        public async Task<List<TopProductDto>>
            GetTopProductsAsync(int topN)
        {
            var result = await _context.Database
                .SqlQueryRaw<TopProductDto>($@"
                    SELECT TOP ({topN})
                        ProductId,
                        SUM(CASE WHEN EventType =
                            'ProductViewed' THEN 1
                            ELSE 0 END) AS TotalViews,
                        SUM(CASE WHEN EventType =
                            'AddedToCart' THEN 1
                            ELSE 0 END) AS TotalAddedToCart,
                        SUM(CASE WHEN EventType =
                            'Purchased' THEN 1
                            ELSE 0 END) AS TotalPurchased,
                        CASE
                            WHEN SUM(CASE WHEN EventType =
                                 'ProductViewed' THEN 1
                                 ELSE 0 END) > 0
                            THEN CAST(
                                SUM(CASE WHEN EventType =
                                    'Purchased' THEN 1
                                    ELSE 0 END) AS FLOAT)
                                / SUM(CASE WHEN EventType =
                                    'ProductViewed' THEN 1
                                    ELSE 0 END) * 100
                            ELSE 0
                        END AS ConversionRate
                    FROM ClickEvents
                    WHERE ProductId IS NOT NULL
                    GROUP BY ProductId
                    ORDER BY TotalViews DESC")
                .ToListAsync();

            return result;
        }

        // ── Raw SQL: Peak Hours ───────────────────────
        public async Task<List<HourlyOrderDto>>
            GetPeakHoursAsync()
        {
            var result = await _context.Database
                .SqlQueryRaw<HourlyOrderDto>(@"
                    SELECT
                        DATEPART(HOUR, CreatedAt) AS Hour,
                        COUNT(*) AS TotalOrders,
                        ISNULL(SUM(RevenueAmount), 0)
                            AS TotalRevenue
                    FROM OrderAnalytics
                    WHERE CreatedAt >=
                          DATEADD(DAY, -30, GETUTCDATE())
                    GROUP BY DATEPART(HOUR, CreatedAt)
                    ORDER BY Hour ASC")
                .ToListAsync();

            return result;
        }
    }
}