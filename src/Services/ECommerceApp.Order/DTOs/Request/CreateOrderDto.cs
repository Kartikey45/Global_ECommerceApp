using System.ComponentModel.DataAnnotations;

namespace ECommerceApp.Order.DTOs.Request
{
    public class CreateOrderDto
    {
        [Required]
        [MinLength(1,
            ErrorMessage =
                "Order must have at least one item.")]
        public List<CreateOrderItemDto> Items { get; set; }
            = new();

        [Required]
        [MaxLength(500,
            ErrorMessage =
                "Shipping address is too long.")]
        public string ShippingAddress { get; set; }
            = string.Empty;
    }
}