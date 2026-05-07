using ECommerceApp.Order.DTOs.Response;
using ECommerceApp.Order.Enums;
using ECommerceApp.Order.HttpClients;

namespace ECommerceApp.Order.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        Task<Models.Order?> GetByIdAsync(int id);
        Task<(List<Models.Order> Items, int TotalCount)>
            GetUserOrdersAsync(
                string userId, int page, int pageSize);
        Task<(List<Models.Order> Items, int TotalCount)>
            GetAllOrdersAsync(int page, int pageSize);
        Task<int> CreateOrderAsync(
            Models.Order order,
            List<ProductDto> products);
        Task<bool> UpdateStatusAsync(
            int orderId, OrderStatus status);
        Task<bool> UpdateShippingAsync(
            int orderId,
            string trackingNumber,
            string carrier);
        Task<bool> ExistsAsync(int id);
    }
}