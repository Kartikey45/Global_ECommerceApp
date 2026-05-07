using System.ComponentModel.DataAnnotations;

namespace ECommerceApp.Order.DTOs.Request
{
    public class CancelOrderDto
    {
        [Required]
        [MaxLength(200)]
        public string Reason { get; set; }
            = string.Empty;
    }
}