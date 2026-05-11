namespace ECommerceApp.Shipping.Services.Interfaces
{
    public interface ICarrierService
    {
        Task<CarrierResult> BookShipmentAsync(
            int orderId,
            string deliveryAddress);
    }

    public class CarrierResult
    {
        public string TrackingNumber { get; set; }
            = string.Empty;
        public string Carrier { get; set; }
            = string.Empty;
        public DateTime EstimatedDelivery { get; set; }
    }
}