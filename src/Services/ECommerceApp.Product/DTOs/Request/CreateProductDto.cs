using System.ComponentModel.DataAnnotations;

namespace ECommerceApp.Product.DTOs.Request
{
    public class CreateProductDto
    {
        [Required(ErrorMessage = "Name is required.")]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue,
            ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue,
            ErrorMessage = "Stock cannot be negative.")]
        public int StockQuantity { get; set; }

        [Required]
        [MaxLength(50)]
        public string SKU { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "Category is required.")]
        public int CategoryId { get; set; }
    }
}