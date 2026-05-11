using ECommerceApp.Shipping.Services.Interfaces;

namespace ECommerceApp.Shipping.Services.Implementations
{
    /// <summary>
    /// Mock FedEx/UPS carrier service for local dev.
    /// Replace with real carrier SDK in production.
    /// </summary>
    public class CarrierService : ICarrierService
    {
        private readonly ILogger<CarrierService> _logger;

        // Available carriers
        private static readonly string[] Carriers =
        {
            "FedEx", "UPS", "DHL"
        };

        public CarrierService(
            ILogger<CarrierService> logger)
        {
            _logger = logger;
        }

        public async Task<CarrierResult>
            BookShipmentAsync(
            int orderId,
            string deliveryAddress)
        {
            // Simulate carrier API network delay
            await Task.Delay(
                TimeSpan.FromMilliseconds(
                    Random.Shared.Next(200, 800)));

            // Randomly pick a carrier
            var carrier = Carriers[
                Random.Shared.Next(Carriers.Length)];

            // Generate tracking number
            var trackingNumber =
                $"{carrier.ToUpper()}-" +
                $"{DateTime.UtcNow:yyyyMMdd}-" +
                $"{orderId:D4}-" +
                $"{Random.Shared.Next(10000, 99999)}";

            // Estimated delivery: 3-7 business days
            var estimatedDelivery =
                DateTime.UtcNow.AddDays(
                    Random.Shared.Next(3, 8));

            _logger.LogInformation(
                "Carrier MOCK: Shipment booked — " +
                "OrderId: {OrderId}, " +
                "Carrier: {Carrier}, " +
                "Tracking: {Tracking}, " +
                "ETA: {ETA}",
                orderId, carrier,
                trackingNumber, estimatedDelivery);

            return new CarrierResult
            {
                TrackingNumber = trackingNumber,
                Carrier = carrier,
                EstimatedDelivery = estimatedDelivery
            };
        }
    }
}