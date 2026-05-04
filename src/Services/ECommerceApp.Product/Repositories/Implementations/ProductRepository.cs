using ECommerceApp.Product.Data;
using ECommerceApp.Product.DTOs.Response;
using ECommerceApp.Product.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ECommerceApp.Product.Repositories.Implementations
{
    public class ProductRepository : IProductRepository
    {
        private readonly ProductDbContext _context;
        private readonly ILogger<ProductRepository> _logger;

        public ProductRepository(
            ProductDbContext context,
            ILogger<ProductRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ── Stored Procedure: Paged Search ───────────────────
        public async Task<(List<ProductPagedResult> Items,
            int TotalCount)> GetAllPagedAsync(
            int page, int pageSize,
            int? categoryId, decimal? minPrice,
            decimal? maxPrice, string? search)
        {
            var pageParam = new SqlParameter(
                "@Page", page);
            var pageSizeParam = new SqlParameter(
                "@PageSize", pageSize);
            var categoryParam = new SqlParameter(
                "@CategoryId",
                categoryId.HasValue
                    ? categoryId.Value
                    : DBNull.Value);
            var minPriceParam = new SqlParameter(
                "@MinPrice",
                minPrice.HasValue
                    ? minPrice.Value
                    : DBNull.Value);
            var maxPriceParam = new SqlParameter(
                "@MaxPrice",
                maxPrice.HasValue
                    ? maxPrice.Value
                    : DBNull.Value);
            var searchParam = new SqlParameter(
                "@Search",
                string.IsNullOrWhiteSpace(search)
                    ? DBNull.Value
                    : search.Trim());
            var totalCountParam = new SqlParameter
            {
                ParameterName = "@TotalCount",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output
            };

            var items = await _context
                .Set<ProductPagedResult>()
                .FromSqlRaw(
                    "EXEC usp_GetProductsPaged " +
                    "@Page, @PageSize, @CategoryId, " +
                    "@MinPrice, @MaxPrice, @Search, " +
                    "@TotalCount OUTPUT",
                    pageParam, pageSizeParam,
                    categoryParam, minPriceParam,
                    maxPriceParam, searchParam,
                    totalCountParam)
                .ToListAsync();

            var total = totalCountParam.Value != DBNull.Value
                ? (int)totalCountParam.Value
                : 0;

            return (items, total);
        }

        // ── EF Core: Get by ID ────────────────────────────────
        public async Task<Models.Product?> GetByIdAsync(int id)
            => await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(
                    p => p.Id == id && p.IsActive);

        // ── EF Core: Get by SKU ───────────────────────────────
        public async Task<Models.Product?> GetBySkuAsync(
            string sku)
            => await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(
                    p => p.SKU == sku && p.IsActive);

        // ── EF Core: Exists ───────────────────────────────────
        public async Task<bool> ExistsAsync(int id)
            => await _context.Products
                .AnyAsync(p => p.Id == id && p.IsActive);

        public async Task<bool> SkuExistsAsync(
            string sku, int? excludeId = null)
        {
            var query = _context.Products
                .Where(p => p.SKU == sku);

            if (excludeId.HasValue)
                query = query.Where(
                    p => p.Id != excludeId.Value);

            return await query.AnyAsync();
        }

        // ── EF Core: Create ───────────────────────────────────
        public async Task<Models.Product> CreateAsync(
            Models.Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Reload with category navigation property
            return (await GetByIdAsync(product.Id))!;
        }

        // ── EF Core: Update ───────────────────────────────────
        public async Task<Models.Product?> UpdateAsync(
            int id, Models.Product updated)
        {
            var product = await _context.Products
                .FindAsync(id);

            if (product == null) return null;

            product.Name = updated.Name;
            product.Description = updated.Description;
            product.Price = updated.Price;
            product.StockQuantity = updated.StockQuantity;
            product.ImageUrl = updated.ImageUrl;
            product.CategoryId = updated.CategoryId;
            product.IsActive = updated.IsActive;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await GetByIdAsync(id);
        }

        // ── EF Core: Soft Delete ──────────────────────────────
        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _context.Products
                .FindAsync(id);

            if (product == null) return false;

            product.IsActive = false;
            product.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        // ── Stored Procedure: Atomic Stock Update ─────────────
        public async Task<(bool Success, string Message)>
            UpdateStockAsync(int productId, int requestedQty)
        {
            var productIdParam = new SqlParameter(
                "@ProductId", productId);
            var qtyParam = new SqlParameter(
                "@RequestedQty", requestedQty);
            var successParam = new SqlParameter
            {
                ParameterName = "@Success",
                SqlDbType = SqlDbType.Bit,
                Direction = ParameterDirection.Output
            };
            var messageParam = new SqlParameter
            {
                ParameterName = "@Message",
                SqlDbType = SqlDbType.NVarChar,
                Size = 200,
                Direction = ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC usp_UpdateProductStock " +
                "@ProductId, @RequestedQty, " +
                "@Success OUTPUT, @Message OUTPUT",
                productIdParam, qtyParam,
                successParam, messageParam);

            var success = successParam.Value != DBNull.Value
                && (bool)successParam.Value;
            var message = messageParam.Value != DBNull.Value
                ? (string)messageParam.Value
                : "Unknown error.";

            return (success, message);
        }
    }
}