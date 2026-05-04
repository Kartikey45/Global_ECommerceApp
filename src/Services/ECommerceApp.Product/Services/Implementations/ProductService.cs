using ECommerceApp.Product.DTOs.Request;
using ECommerceApp.Product.DTOs.Response;
using ECommerceApp.Product.Repositories.Interfaces;
using ECommerceApp.Product.Services.Interfaces;

namespace ECommerceApp.Product.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repo;
        private readonly ICacheService _cache;
        private readonly ILogger<ProductService> _logger;

        // Cache key prefixes
        private const string SinglePrefix = "single:";
        private const string ListPrefix = "list:";

        public ProductService(
            IProductRepository repo,
            ICacheService cache,
            ILogger<ProductService> logger)
        {
            _repo = repo;
            _cache = cache;
            _logger = logger;
        }

        public async Task<PagedProductResponse> GetAllAsync(
            int page, int pageSize,
            int? categoryId, decimal? minPrice,
            decimal? maxPrice, string? search)
        {
            var cacheKey =
                $"{ListPrefix}p{page}_ps{pageSize}" +
                $"_cat{categoryId}_min{minPrice}" +
                $"_max{maxPrice}" +
                $"_s{search?.ToLower().Trim()}";

            return await _cache.GetOrSetAsync(
                cacheKey,
                async () =>
                {
                    var (items, total) =
                        await _repo.GetAllPagedAsync(
                            page, pageSize, categoryId,
                            minPrice, maxPrice, search);

                    return new PagedProductResponse
                    {
                        Items = items.Select(
                            MapPagedToDto).ToList(),
                        TotalCount = total,
                        Page = page,
                        PageSize = pageSize
                    };
                },
                TimeSpan.FromMinutes(30));
        }

        public async Task<ProductResponseDto?> GetByIdAsync(
            int id)
        {
            var cacheKey = $"{SinglePrefix}{id}";

            return await _cache.GetOrSetAsync(
                cacheKey,
                async () =>
                {
                    var product =
                        await _repo.GetByIdAsync(id);
                    return product == null
                        ? null!
                        : MapToDto(product);
                },
                TimeSpan.FromHours(1));
        }

        public async Task<ProductResponseDto?> GetBySkuAsync(
            string sku)
        {
            var product = await _repo.GetBySkuAsync(sku);
            return product == null ? null : MapToDto(product);
        }

        public async Task<ProductResponseDto> CreateAsync(
            CreateProductDto dto)
        {
            // Validate SKU uniqueness
            if (await _repo.SkuExistsAsync(dto.SKU))
                throw new InvalidOperationException(
                    $"SKU '{dto.SKU}' already exists.");

            var product = new Models.Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                StockQuantity = dto.StockQuantity,
                SKU = dto.SKU.ToUpper().Trim(),
                ImageUrl = dto.ImageUrl,
                CategoryId = dto.CategoryId,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _repo.CreateAsync(product);

            // Bust all list caches
            await _cache.RemoveByPrefixAsync(ListPrefix);

            _logger.LogInformation(
                "Product created — Id: {Id} SKU: {SKU}",
                created.Id, created.SKU);

            return MapToDto(created);
        }

        public async Task<ProductResponseDto?> UpdateAsync(
            int id, UpdateProductDto dto)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return null;

            // Apply only provided fields (PATCH-style)
            existing.Name =
                dto.Name ?? existing.Name;
            existing.Description =
                dto.Description ?? existing.Description;
            existing.Price =
                dto.Price ?? existing.Price;
            existing.StockQuantity =
                dto.StockQuantity ?? existing.StockQuantity;
            existing.ImageUrl =
                dto.ImageUrl ?? existing.ImageUrl;
            existing.CategoryId =
                dto.CategoryId ?? existing.CategoryId;
            existing.IsActive =
                dto.IsActive ?? existing.IsActive;

            var updated = await _repo.UpdateAsync(
                id, existing);

            if (updated == null) return null;

            // Bust specific + list caches
            await _cache.RemoveAsync(
                $"{SinglePrefix}{id}");
            await _cache.RemoveByPrefixAsync(ListPrefix);

            return MapToDto(updated);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var result = await _repo.DeleteAsync(id);
            if (!result) return false;

            await _cache.RemoveAsync(
                $"{SinglePrefix}{id}");
            await _cache.RemoveByPrefixAsync(ListPrefix);

            return true;
        }

        public async Task<(bool Success, string Message)>
            UpdateStockAsync(int id, int quantity)
        {
            if (quantity < 1)
                throw new ArgumentException(
                    "Quantity must be at least 1.");

            var result =
                await _repo.UpdateStockAsync(id, quantity);

            // Bust single product cache on stock change
            if (result.Success)
                await _cache.RemoveAsync(
                    $"{SinglePrefix}{id}");

            return result;
        }

        // ── Mappers ─────────────────────────────────────────
        private static ProductResponseDto MapToDto(
            Models.Product p) => new()
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                SKU = p.SKU,
                ImageUrl = p.ImageUrl,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.Name
                            ?? string.Empty,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            };

        private static ProductResponseDto MapPagedToDto(
            ProductPagedResult p) => new()
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                SKU = p.SKU,
                ImageUrl = p.ImageUrl,
                CategoryId = p.CategoryId,
                CategoryName = p.CategoryName,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            };
    }
}