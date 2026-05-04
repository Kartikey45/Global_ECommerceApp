using ECommerceApp.Product.DTOs.Request;
using ECommerceApp.Product.DTOs.Response;
using ECommerceApp.Product.Repositories.Interfaces;
using ECommerceApp.Product.Services.Interfaces;

namespace ECommerceApp.Product.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repo;
        private readonly ICacheService _cache;
        private const string CacheKey = "category:all";

        public CategoryService(
            ICategoryRepository repo,
            ICacheService cache)
        {
            _repo = repo;
            _cache = cache;
        }

        public async Task<List<CategoryResponseDto>>
            GetAllAsync()
        {
            return await _cache.GetOrSetAsync(
                CacheKey,
                async () =>
                {
                    var cats = await _repo.GetAllAsync();
                    return cats.Select(MapToDto).ToList();
                },
                TimeSpan.FromHours(1));
        }

        public async Task<CategoryResponseDto?> GetByIdAsync(
            int id)
        {
            var cat = await _repo.GetByIdAsync(id);
            return cat == null ? null : MapToDto(cat);
        }

        public async Task<CategoryResponseDto> CreateAsync(
            CreateCategoryDto dto)
        {
            var existing =
                await _repo.GetBySlugAsync(dto.Slug);

            if (existing != null)
                throw new InvalidOperationException(
                    $"Slug '{dto.Slug}' already exists.");

            var category = new Models.Category
            {
                Name = dto.Name,
                Slug = dto.Slug
                                      .ToLower().Trim(),
                ParentCategoryId = dto.ParentCategoryId,
                CreatedAt = DateTime.UtcNow
            };

            var created =
                await _repo.CreateAsync(category);

            // Invalidate category cache
            await _cache.RemoveAsync(CacheKey);

            return MapToDto(created);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var result = await _repo.DeleteAsync(id);
            if (result)
                await _cache.RemoveAsync(CacheKey);
            return result;
        }

        private static CategoryResponseDto MapToDto(
            Models.Category c) => new()
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                ParentCategoryId = c.ParentCategoryId,
                ParentCategoryName =
                c.ParentCategory?.Name,
                IsActive = c.IsActive,
                SubCategories = c.SubCategories
                .Where(s => s.IsActive)
                .Select(MapToDto)
                .ToList()
            };
    }
}