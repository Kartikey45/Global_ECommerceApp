using ECommerceApp.Analytics.DTOs.Request;
using ECommerceApp.Analytics.DTOs.Response;

namespace ECommerceApp.Analytics.Services.Interfaces
{
    public interface IAnalyticsService
    {
        Task TrackClickAsync(
            TrackClickEventDto dto,
            string? userId);

        Task<DashboardSummaryDto> GetDashboardAsync();

        Task<OrderSummaryDto> GetOrderSummaryAsync(
            DateTime? startDate,
            DateTime? endDate);

        Task<List<DailyRevenueDto>> GetDailyRevenueAsync(
            int days = 7);

        Task<List<TopProductDto>> GetTopProductsAsync(
            int topN = 10);

        Task<List<HourlyOrderDto>> GetPeakHoursAsync();

        Task SaveOrderAsync(
            int orderId,
            string userId,
            string userEmail,
            decimal totalAmount,
            int itemCount);

        Task UpdatePaymentAsync(
            int orderId,
            bool paymentSuccess,
            decimal? revenueAmount);
    }
}