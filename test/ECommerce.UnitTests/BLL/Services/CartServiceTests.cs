using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECommerce.BLL.Dtos;
using ECommerce.BLL.Exceptions;
using ECommerce.BLL.Services;
using ECommerce.DAL.Data.RepositoryContracts;
using ECommerce.DAL.Entities;
using Moq;

namespace ECommerce.UnitTests.BLL.Services;

public class CartServiceTests
{
    private readonly Mock<ICartRepository> _mockCartRepository;
    private readonly Mock<IProductRepository> _mockProductRepository;
    private readonly CartService _cartService;
    private readonly string _userId = "test-user-id";

    public CartServiceTests()
    {
        _mockCartRepository = new Mock<ICartRepository>();
        _mockProductRepository = new Mock<IProductRepository>();
        _cartService = new CartService(_mockCartRepository.Object, _mockProductRepository.Object);
    }

    #region GetCartAsync Tests

    [Fact]
    public async Task GetCartAsync_ExistingCart_ReturnsCartDto()
    {
        // Arrange
        var cart = CreateTestCart();
        _mockCartRepository.Setup(r => r.GetCartByUserIdAsync(_userId))
            .ReturnsAsync(cart);

        // Act
        var result = await _cartService.GetCartAsync(_userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(cart.Id, result.Id);
        Assert.Equal(cart.UserId, result.UserId);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(3, result.TotalItems);
        Assert.Equal(35.0m, result.TotalPrice);
    }

    [Fact]
    public async Task GetCartAsync_NonExistingCart_CreatesAndReturnsNewCart()
    {
        // Arrange
        var newCart = new Cart { Id = 1, UserId = _userId, Items = new List<CartItem>() };
        _mockCartRepository.Setup(r => r.GetCartByUserIdAsync(_userId))
            .ReturnsAsync((Cart)null!);
        _mockCartRepository.Setup(r => r.CreateCartAsync(_userId))
            .ReturnsAsync(newCart);

        // Act
        var result = await _cartService.GetCartAsync(_userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(newCart.Id, result.Id);
        Assert.Equal(newCart.UserId, result.UserId);
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalItems);
        Assert.Equal(0, result.TotalPrice);
        _mockCartRepository.Verify(r => r.CreateCartAsync(_userId), Times.Once);
    }

    #endregion

    #region AddItemToCartAsync Tests

    [Fact]
    public async Task AddItemToCartAsync_NewItemWithSufficientStock_AddsItemToCart()
    {
        // Arrange
        var cart = CreateTestCart();
        var product = new Product { Id = 3, Name = "Product 3", Price = 15.0m, StockQuantity = 10 };
        var addItemDto = new AddCartItemDto { ProductId = 3, Quantity = 2 };

        _mockCartRepository.Setup(r => r.GetCartByUserIdAsync(_userId)).ReturnsAsync(cart);
        _mockProductRepository.Setup(r => r.GetByIdAsync(addItemDto.ProductId)).ReturnsAsync(product);
        _mockCartRepository.Setup(r => r.GetCartItemAsync(cart.Id, addItemDto.ProductId))
            .ReturnsAsync((CartItem)null!);

        // Act
        await _cartService.AddItemToCartAsync(_userId, addItemDto);

        // Assert
        _mockCartRepository.Verify(r => r.AddItemToCartAsync(
            It.IsAny<Cart>(),
            It.Is<CartItem>(i => i.ProductId == addItemDto.ProductId && i.Quantity == addItemDto.Quantity)),
            Times.Once);
    }

    [Fact]
    public async Task AddItemToCartAsync_ExistingItemWithSufficientStock_UpdatesItemQuantity()
    {
        // Arrange
        var cart = CreateTestCart();
        var product = cart.Items[0].Product;
        var existingItem = cart.Items[0];
        var initialQuantity = existingItem.Quantity;
        var addItemDto = new AddCartItemDto { ProductId = product.Id, Quantity = 2 };

        _mockCartRepository.Setup(r => r.GetCartByUserIdAsync(_userId)).ReturnsAsync(cart);
        _mockProductRepository.Setup(r => r.GetByIdAsync(addItemDto.ProductId)).ReturnsAsync(product);
        _mockCartRepository.Setup(r => r.GetCartItemAsync(cart.Id, addItemDto.ProductId)).ReturnsAsync(existingItem);

        // Act
        await _cartService.AddItemToCartAsync(_userId, addItemDto);

        // Assert
        _mockCartRepository.Verify(r => r.UpdateCartItemAsync(
            It.Is<CartItem>(i => i.Id == existingItem.Id && i.Quantity == initialQuantity + addItemDto.Quantity)),
            Times.Once);
    }

    [Fact]
    public async Task AddItemToCartAsync_InsufficientStock_ThrowsInsufficientStockException()
    {
        // Arrange
        var cart = CreateTestCart();
        var product = new Product { Id = 3, Name = "Product 3", Price = 15.0m, StockQuantity = 1 };
        var addItemDto = new AddCartItemDto { ProductId = 3, Quantity = 2 };

        _mockCartRepository.Setup(r => r.GetCartByUserIdAsync(_userId)).ReturnsAsync(cart);
        _mockProductRepository.Setup(r => r.GetByIdAsync(addItemDto.ProductId)).ReturnsAsync(product);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InsufficientStockException>(
            () => _cartService.AddItemToCartAsync(_userId, addItemDto));

        Assert.Contains("Not enough items in stock", exception.Message);
        Assert.Contains("Available: 1", exception.Message);
        Assert.Contains("Requested: 2", exception.Message);
    }

    [Fact]
    public async Task AddItemToCartAsync_ProductNotFound_ThrowsProductNotFoundException()
    {
        // Arrange
        var cart = CreateTestCart();
        var addItemDto = new AddCartItemDto { ProductId = 999, Quantity = 1 };

        _mockCartRepository.Setup(r => r.GetCartByUserIdAsync(_userId)).ReturnsAsync(cart);
        _mockProductRepository.Setup(r => r.GetByIdAsync(addItemDto.ProductId)).ReturnsAsync((Product)null!);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ProductNotFoundException>(
            () => _cartService.AddItemToCartAsync(_userId, addItemDto));

        Assert.Contains("Product with ID 999 not found", exception.Message);
    }

    #endregion

    #region UpdateCartItemAsync Tests

    [Fact]
    public async Task UpdateCartItemAsync_ValidItemWithSufficientStock_UpdatesItemQuantity()
    {
        // Arrange
        var cart = CreateTestCart();
        var cartItem = cart.Items[0];
        var product = cartItem.Product;
        var updateItemDto = new UpdateCartItemDto { Quantity = 5 };

        _mockCartRepository.Setup(r => r.GetCartByUserIdAsync(_userId)).ReturnsAsync(cart);
        _mockCartRepository.Setup(r => r.GetCartItemByIdAsync(cartItem.Id)).ReturnsAsync(cartItem);
        _mockProductRepository.Setup(r => r.GetByIdAsync(cartItem.ProductId)).ReturnsAsync(product);

        // Act
        await _cartService.UpdateCartItemAsync(_userId, cartItem.Id, updateItemDto);

        // Assert
        _mockCartRepository.Verify(r => r.UpdateCartItemAsync(
            It.Is<CartItem>(i => i.Id == cartItem.Id && i.Quantity == updateItemDto.Quantity)),
            Times.Once);
    }

    [Fact]
    public async Task UpdateCartItemAsync_InsufficientStock_ThrowsInsufficientStockException()
    {
        // Arrange
        var cart = CreateTestCart();
        var cartItem = cart.Items[0];
        var product = cartItem.Product;
        product.StockQuantity = 3;
        var updateItemDto = new UpdateCartItemDto { Quantity = 5 };

        _mockCartRepository.Setup(r => r.GetCartByUserIdAsync(_userId)).ReturnsAsync(cart);
        _mockCartRepository.Setup(r => r.GetCartItemByIdAsync(cartItem.Id)).ReturnsAsync(cartItem);
        _mockProductRepository.Setup(r => r.GetByIdAsync(cartItem.ProductId)).ReturnsAsync(product);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InsufficientStockException>(
            () => _cartService.UpdateCartItemAsync(_userId, cartItem.Id, updateItemDto));

        Assert.Contains("Not enough items in stock", exception.Message);
        Assert.Contains("Available: 3", exception.Message);
        Assert.Contains("Requested: 5", exception.Message);
    }

    [Fact]
    public async Task UpdateCartItemAsync_CartItemNotFound_ThrowsCartItemNotFoundException()
    {
        // Arrange
        var cart = CreateTestCart();
        var itemId = 999;
        var updateItemDto = new UpdateCartItemDto { Quantity = 2 };

        _mockCartRepository.Setup(r => r.GetCartByUserIdAsync(_userId)).ReturnsAsync(cart);
        _mockCartRepository.Setup(r => r.GetCartItemByIdAsync(itemId)).ReturnsAsync((CartItem)null!);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CartItemNotFoundException>(
            () => _cartService.UpdateCartItemAsync(_userId, itemId, updateItemDto));

        Assert.Contains($"Cart item with ID {itemId} not found", exception.Message);
    }

    [Fact]
    public async Task UpdateCartItemAsync_CartItemFromDifferentCart_ThrowsCartItemNotFoundException()
    {
        // Arrange
        var cart = CreateTestCart();
        var itemId = 3;
        var updateItemDto = new UpdateCartItemDto { Quantity = 2 };
        var differentCartItem = new CartItem { Id = itemId, CartId = cart.Id + 1, ProductId = 1, Quantity = 1 };

        _mockCartRepository.Setup(r => r.GetCartByUserIdAsync(_userId)).ReturnsAsync(cart);
        _mockCartRepository.Setup(r => r.GetCartItemByIdAsync(itemId)).ReturnsAsync(differentCartItem);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CartItemNotFoundException>(
            () => _cartService.UpdateCartItemAsync(_userId, itemId, updateItemDto));

        Assert.Contains($"Cart item with ID {itemId} not found in cart", exception.Message);
    }

    [Fact]
    public async Task UpdateCartItemAsync_CartNotFound_ThrowsCartNotFoundException()
    {
        // Arrange
        var itemId = 1;
        var updateItemDto = new UpdateCartItemDto { Quantity = 2 };

        _mockCartRepository.Setup(r => r.GetCartByUserIdAsync(_userId)).ReturnsAsync((Cart)null!);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CartNotFoundException>(
            () => _cartService.UpdateCartItemAsync(_userId, itemId, updateItemDto));

        Assert.Contains($"Cart not found for user {_userId}", exception.Message);
    }

    #endregion

    #region RemoveCartItemAsync Tests

    [Fact]
    public async Task RemoveCartItemAsync_ValidItem_RemovesItem()
    {
        // Arrange
        var cart = CreateTestCart();
        var cartItem = cart.Items[0];

        _mockCartRepository.Setup(r => r.GetCartByUserIdAsync(_userId)).ReturnsAsync(cart);
        _mockCartRepository.Setup(r => r.GetCartItemByIdAsync(cartItem.Id)).ReturnsAsync(cartItem);

        // Act
        await _cartService.RemoveCartItemAsync(_userId, cartItem.Id);

        // Assert
        _mockCartRepository.Verify(r => r.RemoveCartItemAsync(cartItem), Times.Once);
    }

    [Fact]
    public async Task RemoveCartItemAsync_CartItemNotFound_ThrowsCartItemNotFoundException()
    {
        // Arrange
        var cart = CreateTestCart();
        var itemId = 999;

        _mockCartRepository.Setup(r => r.GetCartByUserIdAsync(_userId)).ReturnsAsync(cart);
        _mockCartRepository.Setup(r => r.GetCartItemByIdAsync(itemId)).ReturnsAsync((CartItem)null!);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CartItemNotFoundException>(
            () => _cartService.RemoveCartItemAsync(_userId, itemId));

        Assert.Contains($"Cart item with ID {itemId} not found", exception.Message);
    }

    #endregion

    #region ClearCartAsync Tests

    [Fact]
    public async Task ClearCartAsync_ExistingCart_ClearsAllItems()
    {
        // Arrange
        var cart = CreateTestCart();
        _mockCartRepository.Setup(r => r.GetCartByUserIdAsync(_userId)).ReturnsAsync(cart);

        // Act
        await _cartService.ClearCartAsync(_userId);

        // Assert
        _mockCartRepository.Verify(r => r.ClearCartAsync(cart), Times.Once);
    }

    [Fact]
    public async Task ClearCartAsync_NonExistingCart_CreatesNewCart()
    {
        // Arrange
        var newCart = new Cart { Id = 1, UserId = _userId, Items = new List<CartItem>() };
        _mockCartRepository.Setup(r => r.GetCartByUserIdAsync(_userId)).ReturnsAsync((Cart)null!);
        _mockCartRepository.Setup(r => r.CreateCartAsync(_userId)).ReturnsAsync(newCart);

        // Act
        await _cartService.ClearCartAsync(_userId);

        // Assert
        _mockCartRepository.Verify(r => r.CreateCartAsync(_userId), Times.Once);
        _mockCartRepository.Verify(r => r.ClearCartAsync(It.IsAny<Cart>()), Times.Once);
    }

    #endregion

    #region Helper Methods

    private Cart CreateTestCart()
    {
        var product1 = new Product { Id = 1, Name = "Product 1", Price = 10.0m, StockQuantity = 10, ImageUrl = "image1.jpg" };
        var product2 = new Product { Id = 2, Name = "Product 2", Price = 15.0m, StockQuantity = 5, ImageUrl = "image2.jpg" };

        var cartItems = new List<CartItem>
        {
            new CartItem { Id = 1, CartId = 1, ProductId = 1, Product = product1, Quantity = 2 },
            new CartItem { Id = 2, CartId = 1, ProductId = 2, Product = product2, Quantity = 1 }
        };

        return new Cart
        {
            Id = 1,
            UserId = _userId,
            Items = cartItems
        };
    }

    #endregion
}