using ECommerceApp.Analytics.DTOs.Request;
using ECommerceApp.Analytics.Services.Interfaces;
using ECommerceApp.Shared.Authorization;
using ECommerceApp.Shared.Constants;
using ECommerceApp.Shared.Helpers;
using ECommerceApp.Shared.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApp.Analytics.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _service;

        public AnalyticsController(
            IAnalyticsService service)
        {
            _service = service;
        }

        /// <summary>
        /// Full dashboard summary — Admin only
        /// Orders + Revenue + Top Products + Peak Hours
        /// </summary>
        [HttpGet("dashboard")]
        [Authorize(Roles = Roles.Groups.Admins)]
        [HasPermission(Permission.View)]
        public async Task<IActionResult> GetDashboard()
        {
            var result =
                await _service.GetDashboardAsync();
            return Ok(ApiResponse<object>.Ok(result));
        }

        /// <summary>
        /// Order summary with optional date range
        /// — Admin only
        /// </summary>
        [HttpGet("orders/summary")]
        [Authorize(Roles = Roles.Groups.Admins)]
        [HasPermission(Permission.View)]
        public async Task<IActionResult> GetOrderSummary(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var result =
                await _service.GetOrderSummaryAsync(
                    startDate, endDate);
            return Ok(ApiResponse<object>.Ok(result));
        }

        /// <summary>
        /// Daily revenue for last N days — Admin only
        /// </summary>
        [HttpGet("orders/revenue")]
        [Authorize(Roles = Roles.Groups.Admins)]
        [HasPermission(Permission.View)]
        public async Task<IActionResult> GetDailyRevenue(
            [FromQuery] int days = 7)
        {
            if (days < 1 || days > 365) days = 7;
            var result =
                await _service.GetDailyRevenueAsync(days);
            return Ok(ApiResponse<object>.Ok(result));
        }

        /// <summary>
        /// Top N products by views/purchases — Admin only
        /// </summary>
        [HttpGet("products/top")]
        [Authorize(Roles = Roles.Groups.Admins)]
        [HasPermission(Permission.View)]
        public async Task<IActionResult> GetTopProducts(
            [FromQuery] int topN = 10)
        {
            if (topN < 1 || topN > 50) topN = 10;
            var result =
                await _service.GetTopProductsAsync(topN);
            return Ok(ApiResponse<object>.Ok(result));
        }

        /// <summary>
        /// Peak order hours — Admin only
        /// </summary>
        [HttpGet("orders/peak-hours")]
        [Authorize(Roles = Roles.Groups.Admins)]
        [HasPermission(Permission.View)]
        public async Task<IActionResult> GetPeakHours()
        {
            var result =
                await _service.GetPeakHoursAsync();
            return Ok(ApiResponse<object>.Ok(result));
        }

        /// <summary>
        /// Track click/view event — Public
        /// Called by Angular frontend
        /// </summary>
        [HttpPost("events/track")]
        [AllowAnonymous]
        public async Task<IActionResult> TrackEvent(
            [FromBody] TrackClickEventDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(
                    ApiResponse<object>.Fail(
                        "Validation failed."));

            // Get userId if authenticated
            // Anonymous users can also track
            string? userId = null;

            if (User.Identity?.IsAuthenticated == true)
                userId = User.GetUserId();

            await _service.TrackClickAsync(dto, userId);

            return Ok(ApiResponse<object>
                .Ok(null!,
                    "Event tracked."));
        }
    }
}