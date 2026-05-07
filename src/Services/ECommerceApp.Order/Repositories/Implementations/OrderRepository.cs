using ECommerceApp.Order.Data;
using ECommerceApp.Order.Enums;
using ECommerceApp.Order.HttpClients;
using ECommerceApp.Order.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Text.Json;

namespace ECommerceApp.Order.Repositories.Implementations
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderDbContext _context;
        private readonly ILogger<OrderRepository> _logger;

        public OrderRepository(
            OrderDbContext context,
            ILogger<OrderRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ── EF Core: Get by ID with items ─────────────────
        public async Task<Models.Order?> GetByIdAsync(
            int id)
            => await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);

        // ── EF Core: Get user orders paged ────────────────
        public async Task<(List<Models.Order> Items,
            int TotalCount)> GetUserOrdersAsync(
            string userId, int page, int pageSize)
        {
            var query = _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt);

            var total = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        // ── EF Core: Get all orders (Admin) paged ─────────
        public async Task<(List<Models.Order> Items,
            int TotalCount)> GetAllOrdersAsync(
            int page, int pageSize)
        {
            var query = _context.Orders
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.CreatedAt);

            var total = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        // ── Stored Procedure: Atomic order creation ───────
        public async Task<int> CreateOrderAsync(
            Models.Order order,
            List<ProductDto> products)
        {
            // Build items JSON for SP
            var itemsJson = JsonSerializer.Serialize(
                order.OrderItems.Select(item =>
                {
                    var product = products
                        .First(p =>
                            p.Id == item.ProductId);
                    return new
                    {
                        productId = item.ProductId,
                        productName = product.Name,
                        quantity = item.Quantity,
                        unitPrice = item.UnitPrice,
                        totalPrice = item.TotalPrice
                    };
                }));

            var userIdParam = new SqlParameter(
                "@UserId", order.UserId);
            var emailParam = new SqlParameter(
                "@UserEmail", order.UserEmail);
            var totalParam = new SqlParameter(
                "@TotalAmount", order.TotalAmount);
            var addressParam = new SqlParameter(
                "@ShippingAddress", order.ShippingAddress);
            var itemsParam = new SqlParameter(
                "@ItemsJson", itemsJson);
            var orderIdParam = new SqlParameter
            {
                ParameterName = "@OrderId",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC usp_CreateOrder " +
                "@UserId, @UserEmail, @TotalAmount, " +
                "@ShippingAddress, @ItemsJson, " +
                "@OrderId OUTPUT",
                userIdParam, emailParam, totalParam,
                addressParam, itemsParam, orderIdParam);

            return (int)orderIdParam.Value;
        }

        // ── EF Core: Update order status ──────────────────
        public async Task<bool> UpdateStatusAsync(
            int orderId, OrderStatus status)
        {
            var order = await _context.Orders
                .FindAsync(orderId);

            if (order == null) return false;

            order.Status = status;
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // ── EF Core: Save tracking info ───────────────────
        public async Task<bool> UpdateShippingAsync(
            int orderId,
            string trackingNumber,
            string carrier)
        {
            var order = await _context.Orders
                .FindAsync(orderId);

            if (order == null) return false;

            order.TrackingNumber = trackingNumber;
            order.Carrier = carrier;
            order.Status = OrderStatus.Shipped;
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
            => await _context.Orders
                .AnyAsync(o => o.Id == id);
    }
}