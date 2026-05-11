using ECommerceApp.Shipping.Enums;

namespace ECommerceApp.Shipping.Repositories.Interfaces
{
    public interface IShipmentRepository
    {
        Task<Models.Shipment?> GetByIdAsync(int id);
        Task<Models.Shipment?> GetByOrderIdAsync(
            int orderId);
        Task<Models.Shipment?> GetByTrackingNumberAsync(
            string trackingNumber);
        Task<List<Models.Shipment>> GetByUserIdAsync(
            string userId);
        Task<int> CreateShipmentAsync(
            Models.Shipment shipment);
        Task UpdateStatusAsync(
            int shipmentId,
            ShipmentStatus status);
        Task<bool> ExistsAsync(int orderId);
    }
}