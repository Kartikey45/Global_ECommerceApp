using ECommerceApp.Shipping.Data;
using ECommerceApp.Shipping.Enums;
using ECommerceApp.Shipping.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ECommerceApp.Shipping.Repositories.Implementations
{
    public class ShipmentRepository : IShipmentRepository
    {
        private readonly ShippingDbContext _context;
        private readonly ILogger<ShipmentRepository>
            _logger;

        public ShipmentRepository(
            ShippingDbContext context,
            ILogger<ShipmentRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ── EF Core: Get by ID ────────────────────────────
        public async Task<Models.Shipment?> GetByIdAsync(
            int id)
            => await _context.Shipments
                .FirstOrDefaultAsync(s => s.Id == id);

        // ── EF Core: Get by OrderId ───────────────────────
        public async Task<Models.Shipment?>
            GetByOrderIdAsync(int orderId)
            => await _context.Shipments
                .FirstOrDefaultAsync(
                    s => s.OrderId == orderId);

        // ── EF Core: Get by Tracking Number ──────────────
        public async Task<Models.Shipment?>
            GetByTrackingNumberAsync(string trackingNumber)
            => await _context.Shipments
                .FirstOrDefaultAsync(
                    s => s.TrackingNumber
                         == trackingNumber);

        // ── EF Core: Get by UserId ────────────────────────
        public async Task<List<Models.Shipment>>
            GetByUserIdAsync(string userId)
            => await _context.Shipments
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

        // ── SP: Create Shipment (atomic) ──────────────────
        public async Task<int> CreateShipmentAsync(
            Models.Shipment shipment)
        {
            var orderIdParam = new SqlParameter(
                "@OrderId", shipment.OrderId);
            var userIdParam = new SqlParameter(
                "@UserId", shipment.UserId);
            var emailParam = new SqlParameter(
                "@UserEmail", shipment.UserEmail);
            var trackingParam = new SqlParameter(
                "@TrackingNumber", shipment.TrackingNumber);
            var carrierParam = new SqlParameter(
                "@Carrier", shipment.Carrier);
            var addressParam = new SqlParameter(
                "@DeliveryAddress",
                shipment.DeliveryAddress);
            var etaParam = new SqlParameter(
                "@EstimatedDelivery",
                shipment.EstimatedDelivery);
            var shipmentIdParam = new SqlParameter
            {
                ParameterName = "@ShipmentId",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC usp_CreateShipment " +
                "@OrderId, @UserId, @UserEmail, " +
                "@TrackingNumber, @Carrier, " +
                "@DeliveryAddress, @EstimatedDelivery, " +
                "@ShipmentId OUTPUT",
                orderIdParam, userIdParam, emailParam,
                trackingParam, carrierParam,
                addressParam, etaParam, shipmentIdParam);

            return (int)shipmentIdParam.Value;
        }

        // ── SP: Update Status (atomic) ────────────────────
        public async Task UpdateStatusAsync(
            int shipmentId, ShipmentStatus status)
        {
            var idParam = new SqlParameter(
                "@ShipmentId", shipmentId);
            var statusParam = new SqlParameter(
                "@Status", status.ToString());

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC usp_UpdateShipmentStatus " +
                "@ShipmentId, @Status",
                idParam, statusParam);
        }

        public async Task<bool> ExistsAsync(int orderId)
            => await _context.Shipments
                .AnyAsync(s => s.OrderId == orderId);
    }
}