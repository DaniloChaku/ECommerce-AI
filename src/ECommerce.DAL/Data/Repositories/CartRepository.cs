using ECommerce.DAL.Data.RepositoryContracts;
using ECommerce.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.DAL.Data.Repositories;

public class CartRepository : ICartRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CartRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Cart?> GetCartByUserIdAsync(string userId)
    {
        return await _dbContext.Carts
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.Category)
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }

    public async Task<CartItem?> GetCartItemAsync(int cartId, int productId)
    {
        return await _dbContext.CartItems
            .FirstOrDefaultAsync(ci => ci.CartId == cartId && ci.ProductId == productId);
    }

    public async Task<CartItem?> GetCartItemByIdAsync(int cartItemId)
    {
        return await _dbContext.CartItems
            .Include(ci => ci.Product)
            .FirstOrDefaultAsync(ci => ci.Id == cartItemId);
    }

    public async Task<bool> AddItemToCartAsync(Cart cart, CartItem item)
    {
        cart.Items.Add(item);

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateCartItemAsync(CartItem item)
    {
        _dbContext.CartItems.Update(item);

        var cart = await _dbContext.Carts.FindAsync(item.CartId);
        if (cart != null)
        {
            _dbContext.Carts.Update(cart);
        }

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveCartItemAsync(CartItem item)
    {
        _dbContext.CartItems.Remove(item);

        var cart = await _dbContext.Carts.FindAsync(item.CartId);
        if (cart != null)
        {
            _dbContext.Carts.Update(cart);
        }

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ClearCartAsync(Cart cart)
    {
        var cartItems = await _dbContext.CartItems
            .Where(ci => ci.CartId == cart.Id)
            .ToListAsync();

        _dbContext.CartItems.RemoveRange(cartItems);

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<Cart> CreateCartAsync(string userId)
    {
        var cart = new Cart
        {
            UserId = userId,
        };

        await _dbContext.Carts.AddAsync(cart);
        await _dbContext.SaveChangesAsync();
        return cart;
    }
}