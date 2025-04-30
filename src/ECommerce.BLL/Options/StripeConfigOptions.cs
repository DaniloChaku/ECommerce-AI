using System.ComponentModel.DataAnnotations;

namespace ECommerce.BLL.Options;

public class StripeConfigOptions
{
    public const string SectionName = "Stripe";

    [Required]
    public string ApiKey { get; set; } = string.Empty;

    [Required]
    public string WebhookSecret { get; set; } = string.Empty;

    [Required]
    public string Currency { get; set; } = string.Empty;

    [Required]
    public string SuccessUrl { get; set; } = string.Empty;

    [Required]
    public string CancelUrl { get; set; } = string.Empty;
}