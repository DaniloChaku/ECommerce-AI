using ECommerce.BLL.Dtos;

namespace ECommerce.BLL.ServiceContracts;

public interface ICartService
{
    Task<CartDto> GetCartAsync(string userId);
    Task<CartDto> AddItemToCartAsync(string userId, AddCartItemDto itemDto);
    Task<CartDto> UpdateCartItemAsync(string userId, int itemId, UpdateCartItemDto itemDto);
    Task<CartDto> RemoveCartItemAsync(string userId, int itemId);
    Task<CartDto> ClearCartAsync(string userId);
}