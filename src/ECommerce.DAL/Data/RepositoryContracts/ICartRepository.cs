using ECommerce.DAL.Entities;

namespace ECommerce.DAL.Data.RepositoryContracts;

public interface ICartRepository
{
    Task<Cart?> GetCartByUserIdAsync(string userId);
    Task<CartItem?> GetCartItemAsync(int cartId, int productId);
    Task<CartItem?> GetCartItemByIdAsync(int cartItemId);
    Task<bool> AddItemToCartAsync(Cart cart, CartItem item);
    Task<bool> UpdateCartItemAsync(CartItem item);
    Task<bool> RemoveCartItemAsync(CartItem item);
    Task<bool> ClearCartAsync(Cart cart);
    Task<Cart> CreateCartAsync(string userId);
}