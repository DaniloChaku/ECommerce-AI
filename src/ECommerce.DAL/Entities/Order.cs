using System.ComponentModel.DataAnnotations;
using ECommerce.DAL.Constants;
using ECommerce.DAL.Enums;

namespace ECommerce.DAL.Entities;

public class Order
{
    public int Id { get; set; }

    [Required]
    [MaxLength(EntityConstants.User.NameMaxLength)]
    public string UserId { get; set; } = string.Empty;

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public decimal TotalAmount { get; set; }

    public ICollection<OrderItem> Items { get; set; } = [];

    [MaxLength(500)]
    public string? ShippingAddress { get; set; }

    public string? PaymentIntentId { get; set; }

    public DateTime? PaidAt { get; set; }
}