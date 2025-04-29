namespace ECommerce.BLL.Dtos;

public class CartDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
    public decimal TotalPrice { get; set; }
    public int TotalItems { get; set; }
}