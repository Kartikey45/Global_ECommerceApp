using ECommerceApp.Analytics.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.Analytics.Data
{
    public class AnalyticsDbContext : DbContext
    {
        public AnalyticsDbContext(
            DbContextOptions<AnalyticsDbContext> options)
            : base(options) { }

        public DbSet<ClickEvent> ClickEvents
            => Set<ClickEvent>();

        public DbSet<OrderAnalytic> OrderAnalytics
            => Set<OrderAnalytic>();

        protected override void OnModelCreating(
            ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ── ClickEvent Configuration ────────────────
            builder.Entity<ClickEvent>(e =>
            {
                e.HasKey(c => c.Id);

                e.Property(c => c.EventType)
                    .IsRequired()
                    .HasMaxLength(50);

                // Index for fast queries
                e.HasIndex(c => c.Timestamp);
                e.HasIndex(c => c.UserId);
                e.HasIndex(c => c.ProductId);
                e.HasIndex(c => c.EventType);
            });

            // ── OrderAnalytic Configuration ─────────────
            builder.Entity<OrderAnalytic>(e =>
            {
                e.HasKey(o => o.Id);

                e.Property(o => o.TotalAmount)
                    .HasColumnType("decimal(18,2)");

                e.Property(o => o.RevenueAmount)
                    .HasColumnType("decimal(18,2)");

                // Prevent duplicate analytics per order
                e.HasIndex(o => o.OrderId)
                    .IsUnique();

                // Index for date-based queries
                e.HasIndex(o => o.CreatedAt);
                e.HasIndex(o => o.UserId);
            });
        }
    }
}