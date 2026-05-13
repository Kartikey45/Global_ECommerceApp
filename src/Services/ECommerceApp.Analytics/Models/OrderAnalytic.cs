using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceApp.Analytics.Models
{
    public class OrderAnalytic
    {
        [Key]
        [DatabaseGenerated(
            DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // OrderId from Order Service
        public int OrderId { get; set; }

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
        public decimal TotalAmount { get; set; }

        public int ItemCount { get; set; }

        [MaxLength(50)]
        // Pending / Confirmed / Cancelled / etc.
        public string Status { get; set; }
            = string.Empty;

        public bool PaymentSuccess { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? RevenueAmount { get; set; }

        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}