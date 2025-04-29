using System.ComponentModel.DataAnnotations;

namespace ECommerce.BLL.Options;

public class JwtOptions
{
    public const string SectionName = "JWT";

    [Required]
    public string Issuer { get; set; } = string.Empty;

    [Required]
    public string Audience { get; set; } = string.Empty;

    [Required]
    public int ExpirationMinutes { get; set; }

    [Required]
    public string Secret { get; set; } = string.Empty;
}
