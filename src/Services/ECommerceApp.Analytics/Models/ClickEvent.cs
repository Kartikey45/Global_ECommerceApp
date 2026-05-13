using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceApp.Analytics.Models
{
    public class ClickEvent
    {
        [Key]
        [DatabaseGenerated(
            DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // Nullable — anonymous users can also click
        [MaxLength(450)]
        public string? UserId { get; set; }

        // ProductId being viewed/clicked
        public int? ProductId { get; set; }

        [Required]
        [MaxLength(50)]
        // ProductViewed / AddedToCart / Purchased
        // / SearchPerformed / PageVisited
        public string EventType { get; set; }
            = string.Empty;

        [MaxLength(100)]
        public string? SessionId { get; set; }

        [MaxLength(50)]
        public string? IPAddress { get; set; }

        [MaxLength(200)]
        public string? PageUrl { get; set; }

        public DateTime Timestamp { get; set; }
            = DateTime.UtcNow;
    }
}