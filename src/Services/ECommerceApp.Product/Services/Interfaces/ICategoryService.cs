using ECommerceApp.Product.DTOs.Request;
using ECommerceApp.Product.DTOs.Response;

namespace ECommerceApp.Product.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<List<CategoryResponseDto>> GetAllAsync();
        Task<CategoryResponseDto?> GetByIdAsync(int id);
        Task<CategoryResponseDto> CreateAsync(
            CreateCategoryDto dto);
        Task<bool> DeleteAsync(int id);
    }
}