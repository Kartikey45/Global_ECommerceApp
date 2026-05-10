using ECommerceApp.Payment.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceApp.Payment.Models
{
    public class Payment
    {
        [Key]
        [DatabaseGenerated(
            DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // OrderId from Order Service
        // No FK — cross-service reference
        public int OrderId { get; set; }

        // UserId from JWT token
        [Required]
        [MaxLength(450)]
        public string UserId { get; set; }
            = string.Empty;

        [Required]
        [MaxLength(256)]
        public string UserEmail { get; set; }
            = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(10)]
        public string Currency { get; set; } = "USD";

        [Required]
        public PaymentStatus Status { get; set; }
            = PaymentStatus.Processing;

        // Stripe token reference
        // NEVER store actual card details
        [MaxLength(200)]
        public string? TransactionId { get; set; }

        [MaxLength(500)]
        public string? FailureReason { get; set; }

        // Track how many times payment was retried
        public int RetryCount { get; set; } = 0;

        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}