namespace ECommerceApp.Order.DTOs.Response
{
    public class OrderResponseDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
            = string.Empty;
        public string UserEmail { get; set; }
            = string.Empty;
        public string Status { get; set; }
            = string.Empty;
        public decimal TotalAmount { get; set; }
        public string ShippingAddress { get; set; }
            = string.Empty;
        public string? TrackingNumber { get; set; }
        public string? Carrier { get; set; }
        public List<OrderItemResponseDto> Items { get; set; }
            = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class PagedOrderResponse
    {
        public List<OrderResponseDto> Items { get; set; }
            = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages =>
            (int)Math.Ceiling(
                (double)TotalCount / PageSize);
        public bool HasNextPage => Page < TotalPages;
        public bool HasPreviousPage => Page > 1;
    }
}