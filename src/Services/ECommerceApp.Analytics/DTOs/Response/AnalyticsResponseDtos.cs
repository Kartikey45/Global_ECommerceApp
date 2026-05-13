namespace ECommerceApp.Analytics.DTOs.Response
{
    // ── Order Summary ─────────────────────────────────
    public class OrderSummaryDto
    {
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
        public int SuccessfulPayments { get; set; }
        public int FailedPayments { get; set; }
        public double PaymentSuccessRate { get; set; }
    }

    // ── Daily Revenue ─────────────────────────────────
    public class DailyRevenueDto
    {
        public DateTime Date { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
    }

    // ── Top Product ───────────────────────────────────
    public class TopProductDto
    {
        public int ProductId { get; set; }
        public int TotalViews { get; set; }
        public int TotalAddedToCart { get; set; }
        public int TotalPurchased { get; set; }
        public double ConversionRate { get; set; }
    }

    // ── Hourly Orders ─────────────────────────────────
    public class HourlyOrderDto
    {
        public int Hour { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    // ── User Stats ────────────────────────────────────
    public class UserStatsDto
    {
        public int TotalUsers { get; set; }
        public int NewUsersToday { get; set; }
        public int ActiveUsers { get; set; }
    }

    // ── Dashboard Summary ─────────────────────────────
    public class DashboardSummaryDto
    {
        public OrderSummaryDto OrderSummary { get; set; }
            = new();
        public List<DailyRevenueDto> Last7DaysRevenue
        { get; set; } = new();
        public List<TopProductDto> TopProducts
        { get; set; } = new();
        public List<HourlyOrderDto> PeakHours
        { get; set; } = new();
    }
}