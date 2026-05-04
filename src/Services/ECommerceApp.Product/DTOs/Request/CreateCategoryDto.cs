using System.ComponentModel.DataAnnotations;

namespace ECommerceApp.Product.DTOs.Request
{
    public class CreateCategoryDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Slug { get; set; } = string.Empty;

        public int? ParentCategoryId { get; set; }
    }
}