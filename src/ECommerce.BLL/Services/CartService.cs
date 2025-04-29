using ECommerce.BLL.Dtos;
using ECommerce.DAL.Exceptions;
using ECommerce.BLL.ServiceContracts;
using ECommerce.DAL.Data.RepositoryContracts;
using ECommerce.DAL.Entities;

namespace ECommerce.BLL.Services;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;

    public CartService(
        ICartRepository cartRepository,
        IProductRepository productRepository)
    {
        _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
    }

    public async Task<CartDto> GetCartAsync(string userId)
    {
        var cart = await GetOrCreateCartAsync(userId);
        return MapCartToDto(cart);
    }

    public async Task<CartDto> AddItemToCartAsync(string userId, AddCartItemDto itemDto)
    {
        var cart = await GetOrCreateCartAsync(userId);
        var product = await GetProductAsync(itemDto.ProductId);

        ValidateStock(product, itemDto.Quantity);

        var existingItem = await _cartRepository.GetCartItemAsync(cart.Id, itemDto.ProductId);

        if (existingItem != null)
        {
            existingItem.Quantity += itemDto.Quantity;
            await _cartRepository.UpdateCartItemAsync(existingItem);
        }
        else
        {
            var newItem = new CartItem
            {
                CartId = cart.Id,
                ProductId = itemDto.ProductId,
                Quantity = itemDto.Quantity
            };

            await _cartRepository.AddItemToCartAsync(cart, newItem);
        }

        return await GetCartAsync(userId);
    }

    public async Task<CartDto> UpdateCartItemAsync(string userId, int itemId, UpdateCartItemDto itemDto)
    {
        var cart = await GetValidCartAsync(userId);
        var cartItem = await GetValidCartItemAsync(cart.Id, itemId);
        var product = await GetProductAsync(cartItem.ProductId);

        ValidateStock(product, itemDto.Quantity);

        cartItem.Quantity = itemDto.Quantity;
        await _cartRepository.UpdateCartItemAsync(cartItem);

        return await GetCartAsync(userId);
    }

    public async Task<CartDto> RemoveCartItemAsync(string userId, int itemId)
    {
        var cart = await GetValidCartAsync(userId);
        var cartItem = await GetValidCartItemAsync(cart.Id, itemId);

        await _cartRepository.RemoveCartItemAsync(cartItem);

        return await GetCartAsync(userId);
    }

    public async Task<CartDto> ClearCartAsync(string userId)
    {
        var cart = await GetOrCreateCartAsync(userId);
        await _cartRepository.ClearCartAsync(cart);

        return MapCartToDto(cart);
    }

    private async Task<Cart> GetOrCreateCartAsync(string userId)
    {
        var cart = await _cartRepository.GetCartByUserIdAsync(userId);
        return cart ?? await _cartRepository.CreateCartAsync(userId);
    }

    private async Task<Cart> GetValidCartAsync(string userId)
    {
        var cart = await _cartRepository.GetCartByUserIdAsync(userId);
        return cart ?? throw new CartNotFoundException(userId);
    }

    private async Task<CartItem> GetValidCartItemAsync(int cartId, int itemId)
    {
        var cartItem = await _cartRepository.GetCartItemByIdAsync(itemId);

        if (cartItem == null || cartItem.CartId != cartId)
        {
            throw new CartItemNotFoundException(itemId, cartId);
        }

        return cartItem;
    }

    private async Task<Product> GetProductAsync(int productId)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        return product ?? throw new ProductNotFoundException(productId);
    }

    private static void ValidateStock(Product product, int requestedQuantity)
    {
        if (product.StockQuantity < requestedQuantity)
        {
            throw new InsufficientStockException(product.Id, product.StockQuantity, requestedQuantity);
        }
    }

    private CartDto MapCartToDto(Cart cart)
    {
        var cartItems = cart.Items.Select(MapCartItemToDto).ToList();

        return new CartDto
        {
            Id = cart.Id,
            UserId = cart.UserId,
            Items = cartItems,
            TotalPrice = cartItems.Sum(i => i.TotalPrice),
            TotalItems = cartItems.Sum(i => i.Quantity),
        };
    }

    private CartItemDto MapCartItemToDto(CartItem item)
    {
        return new CartItemDto
        {
            Id = item.Id,
            ProductId = item.ProductId,
            ProductName = item.Product.Name,
            ProductImageUrl = item.Product.ImageUrl,
            UnitPrice = item.Product.Price,
            Quantity = item.Quantity,
            TotalPrice = item.Product.Price * item.Quantity
        };
    }
}