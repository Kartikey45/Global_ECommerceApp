using ECommerceApp.Order.Enums;
using ECommerceApp.Order.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.Order.Data
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(
            DbContextOptions<OrderDbContext> options)
            : base(options) { }

        public DbSet<Models.Order> Orders
            => Set<Models.Order>();

        public DbSet<OrderItem> OrderItems
            => Set<OrderItem>();

        protected override void OnModelCreating(
            ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ── Order Configuration ─────────────────────
            builder.Entity<Models.Order>(e =>
            {
                e.HasKey(o => o.Id);

                e.Property(o => o.UserId)
                    .IsRequired()
                    .HasMaxLength(450);

                e.Property(o => o.UserEmail)
                    .IsRequired()
                    .HasMaxLength(256);

                e.Property(o => o.Status)
                    .IsRequired()
                    .HasConversion<string>();

                e.Property(o => o.TotalAmount)
                    .HasColumnType("decimal(18,2)");

                e.Property(o => o.ShippingAddress)
                    .IsRequired()
                    .HasMaxLength(500);

                e.HasMany(o => o.OrderItems)
                    .WithOne(i => i.Order)
                    .HasForeignKey(i => i.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ── OrderItem Configuration ─────────────────
            builder.Entity<OrderItem>(e =>
            {
                e.HasKey(i => i.Id);

                e.Property(i => i.ProductName)
                    .IsRequired()
                    .HasMaxLength(200);

                e.Property(i => i.UnitPrice)
                    .HasColumnType("decimal(18,2)");

                e.Property(i => i.TotalPrice)
                    .HasColumnType("decimal(18,2)");
            });
        }
    }
}