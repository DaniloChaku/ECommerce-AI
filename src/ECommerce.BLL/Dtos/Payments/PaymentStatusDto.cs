namespace ECommerce.BLL.Dtos.Payments;

public class PaymentStatusDto
{
    public string Status { get; set; } = string.Empty;
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public DateTime? PaidAt { get; set; }
}