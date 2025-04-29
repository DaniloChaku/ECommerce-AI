using System.ComponentModel.DataAnnotations;
using ECommerce.DAL.Constants;

namespace ECommerce.BLL.Dtos;

public class RegisterRequest
{
    [Required]
    [StringLength(EntityConstants.User.NameMaxLength)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(EntityConstants.User.EmailMaxLength)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(EntityConstants.User.PasswordMaxLength, 
        MinimumLength = EntityConstants.User.PasswordMinLength)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty;
}