using System.Security.Claims;
using ECommerce.API.Controllers;
using ECommerce.BLL.Dtos;
using ECommerce.BLL.ServiceContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ECommerce.UnitTests.API.Controllers;

[Authorize(AuthenticationSchemes = "Bearer")]
public class CartControllerTests
{
    private readonly Mock<ICartService> _mockCartService;
    private readonly CartController _controller;
    private readonly string _userId = "test-user-id";
    private readonly ClaimsPrincipal _user;

    public CartControllerTests()
    {
        _mockCartService = new Mock<ICartService>();
        _controller = new CartController(_mockCartService.Object);

        var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, _userId)
            };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        _user = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _user }
        };
    }

    #region GetCart Tests

    [Fact]
    public async Task GetCart_AuthenticatedUser_ReturnsOkWithCart()
    {
        // Arrange
        var cartDto = CreateSampleCartDto();
        _mockCartService.Setup(s => s.GetCartAsync(_userId)).ReturnsAsync(cartDto);

        // Act
        var result = await _controller.GetCart();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedCart = Assert.IsType<CartDto>(okResult.Value);
        Assert.Equal(cartDto.Id, returnedCart.Id);
        Assert.Equal(cartDto.UserId, returnedCart.UserId);
        Assert.Equal(cartDto.TotalItems, returnedCart.TotalItems);
        Assert.Equal(cartDto.TotalPrice, returnedCart.TotalPrice);
    }

    #endregion

    #region AddItemToCart Tests

    [Fact]
    public async Task AddItemToCart_ValidItem_ReturnsOkWithUpdatedCart()
    {
        // Arrange
        var cartDto = CreateSampleCartDto();
        var addItemDto = new AddCartItemDto { ProductId = 3, Quantity = 2 };

        _mockCartService.Setup(s => s.AddItemToCartAsync(_userId, addItemDto))
            .ReturnsAsync(cartDto);

        // Act
        var result = await _controller.AddItemToCart(addItemDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedCart = Assert.IsType<CartDto>(okResult.Value);
        Assert.Equal(cartDto.Id, returnedCart.Id);
    }

    #endregion

    #region UpdateCartItem Tests

    [Fact]
    public async Task UpdateCartItem_ValidUpdate_ReturnsOkWithUpdatedCart()
    {
        // Arrange
        var cartDto = CreateSampleCartDto();
        var itemId = 1;
        var updateItemDto = new UpdateCartItemDto { Quantity = 3 };

        _mockCartService.Setup(s => s.UpdateCartItemAsync(_userId, itemId, updateItemDto))
            .ReturnsAsync(cartDto);

        // Act
        var result = await _controller.UpdateCartItem(itemId, updateItemDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedCart = Assert.IsType<CartDto>(okResult.Value);
        Assert.Equal(cartDto.Id, returnedCart.Id);
    }
    #endregion

    #region RemoveCartItem Tests

    [Fact]
    public async Task RemoveCartItem_ValidItem_ReturnsOkWithUpdatedCart()
    {
        // Arrange
        var cartDto = CreateSampleCartDto();
        var itemId = 1;

        _mockCartService.Setup(s => s.RemoveCartItemAsync(_userId, itemId))
            .ReturnsAsync(cartDto);

        // Act
        var result = await _controller.RemoveCartItem(itemId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedCart = Assert.IsType<CartDto>(okResult.Value);
        Assert.Equal(cartDto.Id, returnedCart.Id);
    }

    #endregion

    #region ClearCart Tests

    [Fact]
    public async Task ClearCart_AuthenticatedUser_ReturnsOkWithEmptyCart()
    {
        // Arrange
        var emptyCartDto = new CartDto
        {
            Id = 1,
            UserId = _userId,
            Items = new List<CartItemDto>(),
            TotalItems = 0,
            TotalPrice = 0
        };

        _mockCartService.Setup(s => s.ClearCartAsync(_userId))
            .ReturnsAsync(emptyCartDto);

        // Act
        var result = await _controller.ClearCart();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedCart = Assert.IsType<CartDto>(okResult.Value);
        Assert.Equal(emptyCartDto.Id, returnedCart.Id);
        Assert.Equal(0, returnedCart.TotalItems);
        Assert.Equal(0, returnedCart.TotalPrice);
        Assert.Empty(returnedCart.Items);
    }

    #endregion

    #region Helper Methods

    private CartDto CreateSampleCartDto()
    {
        var items = new List<CartItemDto>
            {
                new CartItemDto
                {
                    Id = 1,
                    ProductId = 1,
                    ProductName = "Product 1",
                    ProductImageUrl = "image1.jpg",
                    UnitPrice = 10.0m,
                    Quantity = 2,
                    TotalPrice = 20.0m
                },
                new CartItemDto
                {
                    Id = 2,
                    ProductId = 2,
                    ProductName = "Product 2",
                    ProductImageUrl = "image2.jpg",
                    UnitPrice = 15.0m,
                    Quantity = 1,
                    TotalPrice = 15.0m
                }
            };

        return new CartDto
        {
            Id = 1,
            UserId = _userId,
            Items = items,
            TotalItems = 3,
            TotalPrice = 35.0m
        };
    }

    #endregion
}
