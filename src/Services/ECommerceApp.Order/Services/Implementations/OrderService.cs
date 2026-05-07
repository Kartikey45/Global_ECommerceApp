using ECommerceApp.Order.DTOs.Request;
using ECommerceApp.Order.DTOs.Response;
using ECommerceApp.Order.Enums;
using ECommerceApp.Order.HttpClients;
using ECommerceApp.Order.Models;
using ECommerceApp.Order.Repositories.Interfaces;
using ECommerceApp.Order.Services.Interfaces;
using ECommerceApp.Shared.Events;
using MassTransit;

namespace ECommerceApp.Order.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _repo;
        private readonly ProductServiceClient _productClient;
        private readonly IPublishEndpoint _publisher;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IOrderRepository repo,
            ProductServiceClient productClient,
            IPublishEndpoint publisher,
            ILogger<OrderService> logger)
        {
            _repo = repo;
            _productClient = productClient;
            _publisher = publisher;
            _logger = logger;
        }

        // ── Get order by ID ───────────────────────────────
        public async Task<OrderResponseDto?> GetByIdAsync(
            int id, string userId, bool isAdmin)
        {
            var order = await _repo.GetByIdAsync(id);
            if (order == null) return null;

            // Customer can only see their own orders
            if (!isAdmin && order.UserId != userId)
                throw new UnauthorizedAccessException(
                    "You are not authorized " +
                    "to view this order.");

            return MapToDto(order);
        }

        // ── Get logged-in user's orders ───────────────────
        public async Task<PagedOrderResponse>
            GetMyOrdersAsync(
            string userId, int page, int pageSize)
        {
            var (items, total) =
                await _repo.GetUserOrdersAsync(
                    userId, page, pageSize);

            return new PagedOrderResponse
            {
                Items = items.Select(MapToDto).ToList(),
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        // ── Get all orders (Admin) ────────────────────────
        public async Task<PagedOrderResponse>
            GetAllOrdersAsync(int page, int pageSize)
        {
            var (items, total) =
                await _repo.GetAllOrdersAsync(
                    page, pageSize);

            return new PagedOrderResponse
            {
                Items = items.Select(MapToDto).ToList(),
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        // ── Create order ──────────────────────────────────
        public async Task<OrderResponseDto> CreateOrderAsync(
            CreateOrderDto dto,
            string userId,
            string userEmail)
        {
            // ── Step 1: Validate & fetch products ─────────
            var products = new List<ProductDto>();

            foreach (var item in dto.Items)
            {
                var product = await _productClient
                    .GetProductAsync(item.ProductId);

                if (product == null)
                    throw new KeyNotFoundException(
                        $"Product {item.ProductId} " +
                        $"not found.");

                if (!product.IsActive)
                    throw new InvalidOperationException(
                        $"Product '{product.Name}' " +
                        $"is not available.");

                if (product.StockQuantity < item.Quantity)
                    throw new InvalidOperationException(
                        $"Insufficient stock for " +
                        $"'{product.Name}'. " +
                        $"Available: {product.StockQuantity}");

                products.Add(product);
            }

            // ── Step 2: Build order entity ─────────────────
            var orderItems = dto.Items.Select(item =>
            {
                var product = products
                    .First(p => p.Id == item.ProductId);

                return new OrderItem
                {
                    ProductId = item.ProductId,
                    ProductName = product.Name,  // snapshot
                    UnitPrice = product.Price, // snapshot
                    Quantity = item.Quantity,
                    TotalPrice =
                        product.Price * item.Quantity
                };
            }).ToList();

            var order = new Models.Order
            {
                UserId = userId,
                UserEmail = userEmail,
                Status = OrderStatus.Pending,
                TotalAmount = orderItems
                    .Sum(i => i.TotalPrice),
                ShippingAddress = dto.ShippingAddress,
                OrderItems = orderItems,
                CreatedAt = DateTime.UtcNow
            };

            // ── Step 3: Save atomically via SP ─────────────
            var orderId = await _repo.CreateOrderAsync(
                order, products);

            order.Id = orderId;

            // ── Step 4: Deduct stock in Product Service ────
            foreach (var item in dto.Items)
            {
                var stockUpdated = await _productClient
                    .UpdateStockAsync(
                        item.ProductId, item.Quantity);

                if (!stockUpdated)
                    _logger.LogWarning(
                        "Stock update failed for " +
                        "product {Id} — manual review needed.",
                        item.ProductId);
            }

            // ── Step 5: Publish OrderPlacedEvent ──────────
            await _publisher.Publish(new OrderPlacedEvent
            {
                OrderId = orderId,
                UserId = userId,
                UserEmail = userEmail,
                TotalAmount = order.TotalAmount,
                ShippingAddress = dto.ShippingAddress,
                Items = orderItems.Select(i =>
                    new OrderItemEvent
                    {
                        ProductId = i.ProductId,
                        ProductName = i.ProductName,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        TotalPrice = i.TotalPrice
                    }).ToList(),
                PlacedAt = DateTime.UtcNow
            });

            _logger.LogInformation(
                "Order {OrderId} created for user {UserId}",
                orderId, userId);

            return MapToDto(order);
        }

        // ── Cancel order ──────────────────────────────────
        public async Task<bool> CancelOrderAsync(
            int id, string userId,
            bool isAdmin, CancelOrderDto dto)
        {
            var order = await _repo.GetByIdAsync(id);
            if (order == null) return false;

            // Only owner or admin can cancel
            if (!isAdmin && order.UserId != userId)
                throw new UnauthorizedAccessException(
                    "You are not authorized " +
                    "to cancel this order.");

            // Can only cancel Pending or Confirmed orders
            if (order.Status != OrderStatus.Pending &&
                order.Status != OrderStatus.Confirmed)
                throw new InvalidOperationException(
                    $"Order cannot be cancelled. " +
                    $"Current status: {order.Status}");

            await _repo.UpdateStatusAsync(
                id, OrderStatus.Cancelled);

            // Publish cancellation event
            await _publisher.Publish(
                new OrderCancelledEvent
                {
                    OrderId = id,
                    UserId = userId,
                    UserEmail = order.UserEmail,
                    Reason = dto.Reason,
                    RefundRequired =
                        order.Status == OrderStatus.Confirmed,
                    RefundAmount = order.TotalAmount,
                    CancelledAt = DateTime.UtcNow
                });

            return true;
        }

        // ── Update status (called by consumers) ───────────
        public async Task<bool> UpdateStatusAsync(
            int orderId, OrderStatus status)
            => await _repo.UpdateStatusAsync(
                orderId, status);

        // ── Update shipping (called by consumers) ─────────
        public async Task<bool> UpdateShippingAsync(
            int orderId,
            string trackingNumber,
            string carrier)
            => await _repo.UpdateShippingAsync(
                orderId, trackingNumber, carrier);

        // ── Mapper ────────────────────────────────────────
        private static OrderResponseDto MapToDto(
            Models.Order o) => new()
            {
                Id = o.Id,
                UserId = o.UserId,
                UserEmail = o.UserEmail,
                Status = o.Status.ToString(),
                TotalAmount = o.TotalAmount,
                ShippingAddress = o.ShippingAddress,
                TrackingNumber = o.TrackingNumber,
                Carrier = o.Carrier,
                Items = o.OrderItems
                .Select(i => new OrderItemResponseDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    UnitPrice = i.UnitPrice,
                    Quantity = i.Quantity,
                    TotalPrice = i.TotalPrice
                }).ToList(),
                CreatedAt = o.CreatedAt,
                UpdatedAt = o.UpdatedAt
            };
    }
}