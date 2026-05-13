using ECommerceApp.Analytics.DTOs.Response;
using ECommerceApp.Analytics.Models;

namespace ECommerceApp.Analytics.Repositories.Interfaces
{
    public interface IAnalyticsRepository
    {
        // ── Click Events ───────────────────────────────
        Task SaveClickEventAsync(ClickEvent clickEvent);

        // ── Order Analytics ────────────────────────────
        Task SaveOrderAnalyticAsync(
            OrderAnalytic analytic);
        Task UpdatePaymentResultAsync(
            int orderId,
            bool paymentSuccess,
            decimal? revenueAmount);

        // ── Query methods ──────────────────────────────
        Task<OrderSummaryDto> GetOrderSummaryAsync(
            DateTime? startDate, DateTime? endDate);
        Task<List<DailyRevenueDto>> GetDailyRevenueAsync(
            int days);
        Task<List<TopProductDto>> GetTopProductsAsync(
            int topN);
        Task<List<HourlyOrderDto>> GetPeakHoursAsync();
    }
}