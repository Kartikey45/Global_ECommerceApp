using ECommerceApp.Analytics.DTOs.Request;
using ECommerceApp.Analytics.DTOs.Response;
using ECommerceApp.Analytics.Models;
using ECommerceApp.Analytics.Repositories.Interfaces;
using ECommerceApp.Analytics.Services.Interfaces;

namespace ECommerceApp.Analytics.Services.Implementations
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IAnalyticsRepository _repo;
        private readonly ILogger<AnalyticsService> _logger;

        public AnalyticsService(
            IAnalyticsRepository repo,
            ILogger<AnalyticsService> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        // ── Track click/view event ────────────────────
        public async Task TrackClickAsync(
            TrackClickEventDto dto,
            string? userId)
        {
            var clickEvent = new ClickEvent
            {
                UserId = userId,
                ProductId = dto.ProductId,
                EventType = dto.EventType,
                SessionId = dto.SessionId,
                PageUrl = dto.PageUrl,
                Timestamp = DateTime.UtcNow
            };

            await _repo.SaveClickEventAsync(clickEvent);

            _logger.LogInformation(
                "Click event tracked — " +
                "Type: {Type}, Product: {ProductId}",
                dto.EventType, dto.ProductId);
        }

        // ── Dashboard summary ─────────────────────────
        public async Task<DashboardSummaryDto>
            GetDashboardAsync()
        {
            var summary =
                await _repo.GetOrderSummaryAsync(
                    DateTime.UtcNow.AddDays(-30),
                    DateTime.UtcNow);

            var last7Days =
                await _repo.GetDailyRevenueAsync(7);

            var topProducts =
                await _repo.GetTopProductsAsync(5);

            var peakHours =
                await _repo.GetPeakHoursAsync();

            return new DashboardSummaryDto
            {
                OrderSummary = summary,
                Last7DaysRevenue = last7Days,
                TopProducts = topProducts,
                PeakHours = peakHours
            };
        }

        public async Task<OrderSummaryDto>
            GetOrderSummaryAsync(
            DateTime? startDate,
            DateTime? endDate) =>
            await _repo.GetOrderSummaryAsync(
                startDate, endDate);

        public async Task<List<DailyRevenueDto>>
            GetDailyRevenueAsync(int days = 7) =>
            await _repo.GetDailyRevenueAsync(days);

        public async Task<List<TopProductDto>>
            GetTopProductsAsync(int topN = 10) =>
            await _repo.GetTopProductsAsync(topN);

        public async Task<List<HourlyOrderDto>>
            GetPeakHoursAsync() =>
            await _repo.GetPeakHoursAsync();

        // ── Called by consumers ───────────────────────
        public async Task SaveOrderAsync(
            int orderId,
            string userId,
            string userEmail,
            decimal totalAmount,
            int itemCount)
        {
            var analytic = new OrderAnalytic
            {
                OrderId = orderId,
                UserId = userId,
                UserEmail = userEmail,
                TotalAmount = totalAmount,
                ItemCount = itemCount,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            await _repo.SaveOrderAnalyticAsync(analytic);
        }

        public async Task UpdatePaymentAsync(
            int orderId,
            bool paymentSuccess,
            decimal? revenueAmount) =>
            await _repo.UpdatePaymentResultAsync(
                orderId, paymentSuccess, revenueAmount);
    }
}