using ECommerceApp.Shipping.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.Shipping.Data
{
    public class ShippingDbContext : DbContext
    {
        public ShippingDbContext(
            DbContextOptions<ShippingDbContext> options)
            : base(options) { }

        public DbSet<Shipment> Shipments
            => Set<Shipment>();

        protected override void OnModelCreating(
            ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Shipment>(e =>
            {
                e.HasKey(s => s.Id);

                e.Property(s => s.UserId)
                    .IsRequired()
                    .HasMaxLength(450);

                e.Property(s => s.UserEmail)
                    .IsRequired()
                    .HasMaxLength(256);

                e.Property(s => s.TrackingNumber)
                    .IsRequired()
                    .HasMaxLength(100);

                e.HasIndex(s => s.TrackingNumber)
                    .IsUnique();

                e.Property(s => s.Carrier)
                    .IsRequired()
                    .HasMaxLength(50);

                e.Property(s => s.Status)
                    .IsRequired()
                    .HasConversion<string>();

                e.Property(s => s.DeliveryAddress)
                    .IsRequired()
                    .HasMaxLength(500);

                // Quick lookup by OrderId
                e.HasIndex(s => s.OrderId);

                // Quick lookup by UserId
                e.HasIndex(s => s.UserId);
            });
        }
    }
}