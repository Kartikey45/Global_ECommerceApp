using ECommerceApp.Product.DTOs.Response;
using ECommerceApp.Product.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.Product.Data
{
    public class ProductDbContext : DbContext
    {
        public ProductDbContext(
            DbContextOptions<ProductDbContext> options)
            : base(options) { }

        public DbSet<Models.Product> Products
            => Set<Models.Product>();

        public DbSet<Category> Categories
            => Set<Category>();

        protected override void OnModelCreating(
            ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ── Category Configuration ──────────────────────
            builder.Entity<Category>(e =>
            {
                e.HasKey(c => c.Id);

                e.Property(c => c.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                e.Property(c => c.Slug)
                    .IsRequired()
                    .HasMaxLength(100);

                e.HasIndex(c => c.Slug)
                    .IsUnique();

                e.HasOne(c => c.ParentCategory)
                    .WithMany(c => c.SubCategories)
                    .HasForeignKey(c => c.ParentCategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ── Product Configuration ───────────────────────
            builder.Entity<Models.Product>(e =>
            {
                e.HasKey(p => p.Id);

                e.Property(p => p.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                e.Property(p => p.Description)
                    .HasMaxLength(2000);

                e.Property(p => p.Price)
                    .HasColumnType("decimal(18,2)");

                e.Property(p => p.SKU)
                    .IsRequired()
                    .HasMaxLength(50);

                e.HasIndex(p => p.SKU)
                    .IsUnique();

                e.HasOne(p => p.Category)
                    .WithMany(c => c.Products)
                    .HasForeignKey(p => p.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ── Seed Categories ─────────────────────────────
            var seedDate = new DateTime(
                2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            builder.Entity<Category>().HasData(
                new Category
                {
                    Id = 1,
                    Name = "Electronics",
                    Slug = "electronics",
                    IsActive = true,
                    CreatedAt = seedDate
                },
                new Category
                {
                    Id = 2,
                    Name = "Clothing",
                    Slug = "clothing",
                    IsActive = true,
                    CreatedAt = seedDate
                },
                new Category
                {
                    Id = 3,
                    Name = "Books",
                    Slug = "books",
                    IsActive = true,
                    CreatedAt = seedDate
                },
                new Category
                {
                    Id = 4,
                    Name = "Home & Kitchen",
                    Slug = "home-kitchen",
                    IsActive = true,
                    CreatedAt = seedDate
                },
                new Category
                {
                    Id = 5,
                    Name = "Phones",
                    Slug = "phones",
                    ParentCategoryId = 1,
                    IsActive = true,
                    CreatedAt = seedDate
                },
                new Category
                {
                    Id = 6,
                    Name = "Laptops",
                    Slug = "laptops",
                    ParentCategoryId = 1,
                    IsActive = true,
                    CreatedAt = seedDate
                }
            );

            // ── Seed Products ───────────────────────────────
            builder.Entity<Models.Product>().HasData(
                new Models.Product
                {
                    Id = 1,
                    Name = "iPhone 15 Pro",
                    Description = "Apple iPhone 15 Pro 256GB",
                    Price = 999.99m,
                    StockQuantity = 100,
                    SKU = "APPL-IPH15P-256",
                    CategoryId = 5,
                    IsActive = true,
                    CreatedAt = seedDate
                },
                new Models.Product
                {
                    Id = 2,
                    Name = "Samsung Galaxy S24",
                    Description = "Samsung Galaxy S24 128GB",
                    Price = 799.99m,
                    StockQuantity = 80,
                    SKU = "SAMS-GS24-128",
                    CategoryId = 5,
                    IsActive = true,
                    CreatedAt = seedDate
                },
                new Models.Product
                {
                    Id = 3,
                    Name = "MacBook Pro 14",
                    Description = "Apple MacBook Pro 14-inch M3",
                    Price = 1999.99m,
                    StockQuantity = 50,
                    SKU = "APPL-MBP14-M3",
                    CategoryId = 6,
                    IsActive = true,
                    CreatedAt = seedDate
                }
            );

            // Required for Stored Procedure result mapping
            // Keyless = no primary key, read-only result set
            builder.Entity<ProductPagedResult>()
                .HasNoKey()
                .ToView(null); // not mapped to a real table
        }
    }
}