using ECommerceApp.Product.Data;
using ECommerceApp.Product.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.Product.Repositories.Implementations
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ProductDbContext _context;

        public CategoryRepository(ProductDbContext context)
        {
            _context = context;
        }

        public async Task<List<Models.Category>> GetAllAsync()
            => await _context.Categories
                .Include(c => c.SubCategories)
                .Include(c => c.ParentCategory)
                .Where(c => c.IsActive
                    && c.ParentCategoryId == null)
                .OrderBy(c => c.Name)
                .ToListAsync();

        public async Task<Models.Category?> GetByIdAsync(int id)
            => await _context.Categories
                .Include(c => c.SubCategories)
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(
                    c => c.Id == id && c.IsActive);

        public async Task<Models.Category?> GetBySlugAsync(
            string slug)
            => await _context.Categories
                .FirstOrDefaultAsync(
                    c => c.Slug == slug && c.IsActive);

        public async Task<Models.Category> CreateAsync(
            Models.Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var category = await _context.Categories
                .FindAsync(id);

            if (category == null) return false;

            category.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
            => await _context.Categories
                .AnyAsync(c => c.Id == id && c.IsActive);
    }
}