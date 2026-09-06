using ECommerceApp.Shipping.Enums;
using ECommerceApp.Shipping.Services.Interfaces;
using ECommerceApp.Shared.Authorization;
using ECommerceApp.Shared.Constants;
using ECommerceApp.Shared.Helpers;
using ECommerceApp.Shared.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApp.Shipping.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShippingController : ControllerBase
    {
        private readonly IShippingService _service;

        public ShippingController(
            IShippingService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get shipment by ID
        /// </summary>
        [HttpGet("{id:int}")]
        [Authorize(Roles = Roles.Groups.All)]
        public async Task<IActionResult> GetById(int id)
        {
            var shipment = await _service.GetByIdAsync(id);

            if (shipment == null)
                return NotFound(
                    ApiResponse<object>.Fail(
                        $"Shipment {id} not found."));

            // Customer can only see own shipments
            if (!User.IsAdmin() &&
                shipment.UserId != User.GetUserId())
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        "Not authorized to view " +
                        "this shipment."));

            return Ok(ApiResponse<object>.Ok(shipment));
        }

        /// <summary>
        /// Get shipment by Order ID
        /// </summary>
        [HttpGet("order/{orderId:int}")]
        [Authorize(Roles = Roles.Groups.All)]
        public async Task<IActionResult> GetByOrderId(
            int orderId)
        {
            var shipment =
                await _service.GetByOrderIdAsync(orderId);

            if (shipment == null)
                return NotFound(
                    ApiResponse<object>.Fail(
                        $"Shipment for Order " +
                        $"{orderId} not found."));

            if (!User.IsAdmin() &&
                shipment.UserId != User.GetUserId())
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        "Not authorized to view " +
                        "this shipment."));

            return Ok(ApiResponse<object>.Ok(shipment));
        }

        /// <summary>
        /// Track by tracking number — Public endpoint
        /// No auth required (like FedEx tracking page)
        /// </summary>
        [HttpGet("track/{trackingNumber}")]
        [AllowAnonymous]
        public async Task<IActionResult> Track(
            string trackingNumber)
        {
            var shipment =
                await _service.GetByTrackingAsync(
                    trackingNumber);

            if (shipment == null)
                return NotFound(
                    ApiResponse<object>.Fail(
                        $"Tracking number " +
                        $"'{trackingNumber}' not found."));

            // Public tracking shows limited info
            return Ok(ApiResponse<object>.Ok(new
            {
                trackingNumber =
                    shipment.TrackingNumber,
                carrier = shipment.Carrier,
                status = shipment.Status,
                estimatedDelivery =
                    shipment.EstimatedDelivery,
                shippedAt = shipment.ShippedAt,
                deliveredAt = shipment.DeliveredAt
            }));
        }

        /// <summary>
        /// Get all shipments for logged-in user
        /// </summary>
        [HttpGet("my-shipments")]
        [Authorize(Roles = Roles.Groups.All)]
        public async Task<IActionResult> GetMyShipments()
        {
            var userId = User.GetUserId();
            var shipments =
                await _service.GetByUserIdAsync(userId);

            return Ok(ApiResponse<object>.Ok(shipments));
        }

        /// <summary>
        /// Update shipment status — Admin only
        /// </summary>
        [HttpPut("{id:int}/status")]
        [Authorize(Roles = Roles.Groups.Admins)]
        [HasPermission(Permission.Edit)]
        public async Task<IActionResult> UpdateStatus(
            int id,
            [FromQuery] ShipmentStatus status)
        {
            var result =
                await _service.UpdateStatusAsync(
                    id, status);

            if (!result)
                return NotFound(
                    ApiResponse<object>.Fail(
                        $"Shipment {id} not found."));

            return Ok(ApiResponse<object>.Ok(
                null!,
                $"Shipment status updated " +
                $"to {status}."));
        }
    }
}