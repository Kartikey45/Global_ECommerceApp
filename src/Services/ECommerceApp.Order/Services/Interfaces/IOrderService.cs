using ECommerceApp.Order.DTOs.Request;
using ECommerceApp.Order.DTOs.Response;
using ECommerceApp.Order.Enums;

namespace ECommerceApp.Order.Services.Interfaces
{
    public interface IOrderService
    {
        Task<OrderResponseDto?> GetByIdAsync(
            int id, string userId, bool isAdmin);

        Task<PagedOrderResponse> GetMyOrdersAsync(
            string userId, int page, int pageSize);

        Task<PagedOrderResponse> GetAllOrdersAsync(
            int page, int pageSize);

        Task<OrderResponseDto> CreateOrderAsync(
            CreateOrderDto dto,
            string userId,
            string userEmail);

        Task<bool> CancelOrderAsync(
            int id, string userId,
            bool isAdmin, CancelOrderDto dto);

        Task<bool> UpdateStatusAsync(
            int orderId, OrderStatus status);

        Task<bool> UpdateShippingAsync(
            int orderId,
            string trackingNumber,
            string carrier);
    }
}