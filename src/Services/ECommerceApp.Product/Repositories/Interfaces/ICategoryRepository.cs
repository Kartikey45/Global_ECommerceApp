namespace ECommerceApp.Product.Repositories.Interfaces
{
    public interface ICategoryRepository
    {
        Task<List<Models.Category>> GetAllAsync();
        Task<Models.Category?> GetByIdAsync(int id);
        Task<Models.Category?> GetBySlugAsync(string slug);
        Task<Models.Category> CreateAsync(
            Models.Category category);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}