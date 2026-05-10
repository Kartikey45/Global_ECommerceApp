using ECommerceApp.Payment.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.Payment.Data
{
    public class PaymentDbContext : DbContext
    {
        public PaymentDbContext(
            DbContextOptions<PaymentDbContext> options)
            : base(options) { }

        public DbSet<Models.Payment> Payments
            => Set<Models.Payment>();

        protected override void OnModelCreating(
            ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Models.Payment>(e =>
            {
                e.HasKey(p => p.Id);

                e.Property(p => p.UserId)
                    .IsRequired()
                    .HasMaxLength(450);

                e.Property(p => p.UserEmail)
                    .IsRequired()
                    .HasMaxLength(256);

                e.Property(p => p.Amount)
                    .HasColumnType("decimal(18,2)");

                e.Property(p => p.Currency)
                    .IsRequired()
                    .HasMaxLength(10);

                e.Property(p => p.Status)
                    .IsRequired()
                    .HasConversion<string>();

                e.Property(p => p.TransactionId)
                    .HasMaxLength(200);

                e.Property(p => p.FailureReason)
                    .HasMaxLength(500);

                // Index for quick lookup by OrderId
                e.HasIndex(p => p.OrderId);

                // Index for quick lookup by UserId
                e.HasIndex(p => p.UserId);
            });
        }
    }
}