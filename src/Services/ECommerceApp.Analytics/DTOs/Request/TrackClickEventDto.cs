using System.ComponentModel.DataAnnotations;

namespace ECommerceApp.Analytics.DTOs.Request
{
    public class TrackClickEventDto
    {
        [Required]
        [MaxLength(50)]
        public string EventType { get; set; }
            = string.Empty;

        public int? ProductId { get; set; }

        [MaxLength(100)]
        public string? SessionId { get; set; }

        [MaxLength(200)]
        public string? PageUrl { get; set; }
    }
}