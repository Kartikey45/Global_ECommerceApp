using ECommerceApp.Order.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceApp.Order.Models
{
    public class Order
    {
        [Key]
        [DatabaseGenerated(
            DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // UserId from JWT — no FK (cross-service)
        [Required]
        [MaxLength(450)]
        public string UserId { get; set; }
            = string.Empty;

        [Required]
        [MaxLength(256)]
        public string UserEmail { get; set; }
            = string.Empty;

        [Required]
        public OrderStatus Status { get; set; }
            = OrderStatus.Pending;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        [MaxLength(500)]
        public string ShippingAddress { get; set; }
            = string.Empty;

        [MaxLength(100)]
        public string? TrackingNumber { get; set; }

        [MaxLength(50)]
        public string? Carrier { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; }
            = new List<OrderItem>();

        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}