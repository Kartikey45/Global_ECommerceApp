using ECommerceApp.Product.DTOs.Request;
using ECommerceApp.Product.DTOs.Response;

namespace ECommerceApp.Product.Services.Interfaces
{
    public interface IProductService
    {
        Task<PagedProductResponse> GetAllAsync(
            int page, int pageSize,
            int? categoryId, decimal? minPrice,
            decimal? maxPrice, string? search);

        Task<ProductResponseDto?> GetByIdAsync(int id);
        Task<ProductResponseDto?> GetBySkuAsync(string sku);

        Task<ProductResponseDto> CreateAsync(
            CreateProductDto dto);

        Task<ProductResponseDto?> UpdateAsync(
            int id, UpdateProductDto dto);

        Task<bool> DeleteAsync(int id);

        Task<(bool Success, string Message)>
            UpdateStockAsync(int id, int quantity);
    }
}