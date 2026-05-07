using System.Net.Http.Json;
using System.Text.Json;

namespace ECommerceApp.Order.HttpClients
{
    public class ProductServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProductServiceClient> _logger;

        public ProductServiceClient(
            HttpClient httpClient,
            ILogger<ProductServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        // ── Get product details from Product Service ──────
        public async Task<ProductDto?> GetProductAsync(
            int productId)
        {
            try
            {
                var response = await _httpClient
                    .GetAsync(
                        $"/api/products/{productId}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "Product {Id} not found " +
                        "in Product Service.",
                        productId);
                    return null;
                }

                var result = await response.Content
                    .ReadFromJsonAsync<ProductServiceResponse> ();

                return result?.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to fetch product {Id} " +
                    "from Product Service.", productId);
                return null;
            }
        }

        // ── Deduct stock via Product Service ──────────────
        public async Task<bool> UpdateStockAsync(
            int productId, int quantity)
        {
            try
            {
                var response = await _httpClient
                    .PatchAsJsonAsync(
                        $"/api/products/{productId}/stock",
                        new { quantity });

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to update stock for " +
                    "product {Id}.", productId);
                return false;
            }
        }
    }

    // ── Response wrapper matching Product Service ─────────
    public class ProductServiceResponse
    {
        public bool Success { get; set; }
        public ProductDto? Data { get; set; }
    }

    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
            = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; }
    }
}