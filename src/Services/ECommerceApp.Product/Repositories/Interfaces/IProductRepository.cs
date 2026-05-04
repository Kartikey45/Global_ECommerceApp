using ECommerceApp.Product.DTOs.Response;

namespace ECommerceApp.Product.Repositories.Interfaces
{
    public interface IProductRepository
    {
        // Uses Stored Procedure for paged search
        Task<(List<ProductPagedResult> Items, int TotalCount)>
            GetAllPagedAsync(
                int page, int pageSize,
                int? categoryId, decimal? minPrice,
                decimal? maxPrice, string? search);

        // Uses EF Core for simple reads
        Task<Models.Product?> GetByIdAsync(int id);
        Task<Models.Product?> GetBySkuAsync(string sku);
        Task<bool> ExistsAsync(int id);
        Task<bool> SkuExistsAsync(
            string sku, int? excludeId = null);

        // Uses EF Core for CRUD
        Task<Models.Product> CreateAsync(
            Models.Product product);
        Task<Models.Product?> UpdateAsync(
            int id, Models.Product product);
        Task<bool> DeleteAsync(int id);

        // Uses Stored Procedure for atomic stock update
        Task<(bool Success, string Message)>
            UpdateStockAsync(
                int productId, int requestedQty);
    }
}