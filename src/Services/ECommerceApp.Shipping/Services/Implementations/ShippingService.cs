using ECommerceApp.Shipping.DTOs.Response;
using ECommerceApp.Shipping.Enums;
using ECommerceApp.Shipping.Models;
using ECommerceApp.Shipping.Repositories.Interfaces;
using ECommerceApp.Shipping.Services.Interfaces;
using ECommerceApp.Shared.Events;
using MassTransit;

namespace ECommerceApp.Shipping.Services.Implementations
{
    public class ShippingService : IShippingService
    {
        private readonly IShipmentRepository _repo;
        private readonly ICarrierService _carrier;
        private readonly IPublishEndpoint _publisher;
        private readonly ILogger<ShippingService> _logger;

        public ShippingService(
            IShipmentRepository repo,
            ICarrierService carrier,
            IPublishEndpoint publisher,
            ILogger<ShippingService> logger)
        {
            _repo = repo;
            _carrier = carrier;
            _publisher = publisher;
            _logger = logger;
        }

        public async Task<ShipmentResponseDto?> GetByIdAsync(
            int id)
        {
            var shipment = await _repo.GetByIdAsync(id);
            return shipment == null
                ? null : MapToDto(shipment);
        }

        public async Task<ShipmentResponseDto?>
            GetByOrderIdAsync(int orderId)
        {
            var shipment =
                await _repo.GetByOrderIdAsync(orderId);
            return shipment == null
                ? null : MapToDto(shipment);
        }

        public async Task<ShipmentResponseDto?>
            GetByTrackingAsync(string trackingNumber)
        {
            var shipment =
                await _repo.GetByTrackingNumberAsync(
                    trackingNumber);
            return shipment == null
                ? null : MapToDto(shipment);
        }

        public async Task<List<ShipmentResponseDto>>
            GetByUserIdAsync(string userId)
        {
            var shipments =
                await _repo.GetByUserIdAsync(userId);
            return shipments.Select(MapToDto).ToList();
        }

        // ── Core: Create shipment after payment ───────────
        public async Task CreateShipmentAsync(
            int orderId,
            string userId,
            string userEmail,
            string shippingAddress)
        {
            // Prevent duplicate shipment
            if (await _repo.ExistsAsync(orderId))
            {
                _logger.LogWarning(
                    "Shipment already exists " +
                    "for Order {OrderId}. Skipping.",
                    orderId);
                return;
            }

            // Step 1: Book shipment with carrier mock
            var carrierResult =
                await _carrier.BookShipmentAsync(
                    orderId, shippingAddress);

            // Step 2: Build shipment entity
            var shipment = new Shipment
            {
                OrderId = orderId,
                UserId = userId,
                UserEmail = userEmail,
                TrackingNumber =
                    carrierResult.TrackingNumber,
                Carrier = carrierResult.Carrier,
                Status = ShipmentStatus.Processing,
                DeliveryAddress = shippingAddress,
                EstimatedDelivery =
                    carrierResult.EstimatedDelivery,
                ShippedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            // Step 3: Save atomically via SP
            var shipmentId =
                await _repo.CreateShipmentAsync(shipment);

            shipment.Id = shipmentId;

            _logger.LogInformation(
                "Shipment created — " +
                "OrderId: {OrderId}, " +
                "Tracking: {Tracking}, " +
                "Carrier: {Carrier}",
                orderId,
                carrierResult.TrackingNumber,
                carrierResult.Carrier);

            // Step 4: Publish OrderShippedEvent
            // → Order Service updates status to Shipped
            // → Notification sends tracking email
            await _publisher.Publish(new OrderShippedEvent
            {
                OrderId = orderId,
                UserId = userId,
                UserEmail = userEmail,
                TrackingNumber =
                    carrierResult.TrackingNumber,
                Carrier = carrierResult.Carrier,
                EstimatedDelivery =
                    carrierResult.EstimatedDelivery,
                ShippedAt = DateTime.UtcNow
            });
        }

        // ── Update shipment status ────────────────────────
        public async Task<bool> UpdateStatusAsync(
            int shipmentId,
            ShipmentStatus status)
        {
            var shipment =
                await _repo.GetByIdAsync(shipmentId);

            if (shipment == null) return false;

            await _repo.UpdateStatusAsync(
                shipmentId, status);

            _logger.LogInformation(
                "Shipment {ShipmentId} " +
                "status updated to {Status}",
                shipmentId, status);

            return true;
        }

        private static ShipmentResponseDto MapToDto(
            Shipment s) => new()
            {
                Id = s.Id,
                OrderId = s.OrderId,
                UserId = s.UserId,
                UserEmail = s.UserEmail,
                TrackingNumber = s.TrackingNumber,
                Carrier = s.Carrier,
                Status = s.Status.ToString(),
                DeliveryAddress = s.DeliveryAddress,
                EstimatedDelivery = s.EstimatedDelivery,
                ShippedAt = s.ShippedAt,
                DeliveredAt = s.DeliveredAt,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            };
    }
}