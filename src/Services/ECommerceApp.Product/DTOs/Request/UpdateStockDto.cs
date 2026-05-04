using System.ComponentModel.DataAnnotations;

namespace ECommerceApp.Product.DTOs.Request
{
    public class UpdateStockDto
    {
        [Required]
        [Range(1, int.MaxValue,
            ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }
    }
}