using ECommerceApp.Shipping.DTOs.Response;
using ECommerceApp.Shipping.Enums;

namespace ECommerceApp.Shipping.Services.Interfaces
{
    public interface IShippingService
    {
        Task<ShipmentResponseDto?> GetByIdAsync(int id);
        Task<ShipmentResponseDto?> GetByOrderIdAsync(
            int orderId);
        Task<ShipmentResponseDto?> GetByTrackingAsync(
            string trackingNumber);
        Task<List<ShipmentResponseDto>> GetByUserIdAsync(
            string userId);
        Task CreateShipmentAsync(
            int orderId,
            string userId,
            string userEmail,
            string shippingAddress);
        Task<bool> UpdateStatusAsync(
            int shipmentId,
            ShipmentStatus status);
    }
}