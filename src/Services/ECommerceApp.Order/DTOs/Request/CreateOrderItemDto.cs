using System.ComponentModel.DataAnnotations;

namespace ECommerceApp.Order.DTOs.Request
{
    public class CreateOrderItemDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue,
            ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }
    }
}