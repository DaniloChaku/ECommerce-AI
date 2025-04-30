namespace ECommerce.BLL.Dtos.Payments;

public class PaymentIntentDto
{
    public string ClientSecret { get; set; } = string.Empty;
    public string PaymentIntentId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "usd";
    public int OrderId { get; set; }
}