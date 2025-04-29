namespace ECommerce.BLL.Dtos.Orders;

 public class OrderDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public List<OrderItemDto> Items { get; set; } = [];
        public string? ShippingAddress { get; set; }
        public DateTime? PaidAt { get; set; }
    }