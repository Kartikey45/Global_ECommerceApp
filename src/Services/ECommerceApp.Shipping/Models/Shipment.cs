using ECommerceApp.Shipping.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceApp.Shipping.Models
{
    public class Shipment
    {
        [Key]
        [DatabaseGenerated(
            DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // OrderId from Order Service
        // No FK — cross-service reference
        public int OrderId { get; set; }

        // UserId from event
        [Required]
        [MaxLength(450)]
        public string UserId { get; set; }
            = string.Empty;

        [Required]
        [MaxLength(256)]
        public string UserEmail { get; set; }
            = string.Empty;

        [Required]
        [MaxLength(100)]
        public string TrackingNumber { get; set; }
            = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Carrier { get; set; }
            = string.Empty;

        [Required]
        public ShipmentStatus Status { get; set; }
            = ShipmentStatus.Processing;

        [Required]
        [MaxLength(500)]
        public string DeliveryAddress { get; set; }
            = string.Empty;

        public DateTime EstimatedDelivery { get; set; }

        public DateTime? ShippedAt { get; set; }

        public DateTime? DeliveredAt { get; set; }

        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}