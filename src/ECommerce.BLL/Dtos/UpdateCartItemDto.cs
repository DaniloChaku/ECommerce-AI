using System.ComponentModel.DataAnnotations;

namespace ECommerce.BLL.Dtos;

public class UpdateCartItemDto
{
    [Required]
    [Range(1, 10, ErrorMessage = "Quantity must be between 1 and 10")]
    public int Quantity { get; set; }
}