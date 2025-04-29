using AutoFixture;
using ECommerce.BLL.Dtos.Orders;
using ECommerce.BLL.Services;
using ECommerce.DAL.Data.RepositoryContracts;
using ECommerce.DAL.Entities;
using ECommerce.DAL.Enums;
using ECommerce.DAL.Exceptions;
using Moq;

namespace ECommerce.UnitTests.BLL.Services;

public class OrderServiceTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IOrderRepository> _orderRepoMock;
    private readonly Mock<ICartRepository> _cartRepoMock;
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly OrderService _sut;

    public OrderServiceTests()
    {
        _fixture = new Fixture();
        _fixture.Customize<OrderItem>(composer => composer.With(o => o.Product, (Product)null!));
        _fixture.Customize<OrderItem>(composer => composer.With(o => o.Order, (Order)null!));
        _fixture.Customize<CartItem>(composer => composer.With(o => o.Product, (Product)null!));
        _fixture.Customize<Category>(composer => composer.With(o => o.Products, []));
        _orderRepoMock = new Mock<IOrderRepository>();
        _cartRepoMock = new Mock<ICartRepository>();
        _productRepoMock = new Mock<IProductRepository>();

        _sut = new OrderService(_orderRepoMock.Object, _cartRepoMock.Object, _productRepoMock.Object);
    }

    [Fact]
    public async Task GetOrderByIdAsync_ValidOrder_ReturnsDto()
    {
        var userId = "user1";
        var order = _fixture.Build<Order>().With(o => o.UserId, userId).Create();

        _orderRepoMock.Setup(r => r.GetByIdAsync(order.Id)).ReturnsAsync(order);

        var result = await _sut.GetOrderByIdAsync(order.Id, userId);

        Assert.Equal(order.Id, result.Id);
        Assert.Equal(order.UserId, result.UserId);
    }

    [Fact]
    public async Task GetOrderByIdAsync_OrderNotFound_ThrowsNotFound()
    {
        _orderRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Order?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetOrderByIdAsync(1, "user1"));
    }

    [Fact]
    public async Task GetOrderByIdAsync_UserNotOwner_ThrowsForbidden()
    {
        var order = _fixture.Build<Order>().With(o => o.UserId, "anotherUser").Create();
        _orderRepoMock.Setup(r => r.GetByIdAsync(order.Id)).ReturnsAsync(order);

        await Assert.ThrowsAsync<ForbiddenException>(() => _sut.GetOrderByIdAsync(order.Id, "user1"));
    }

    [Fact]
    public async Task CreateOrderFromCartAsync_ValidCart_ReturnsOrderDto()
    {
        var userId = "user1";
        var createDto = _fixture.Create<CreateOrderDto>();
        var product = _fixture.Create<Product>();
        var cartItem = new CartItem { ProductId = product.Id, Quantity = 1 };
        var cart = new Cart { Items = new List<CartItem> { cartItem } };
        var order = _fixture.Create<Order>();

        _cartRepoMock.Setup(r => r.GetCartByUserIdAsync(userId)).ReturnsAsync(cart);
        _productRepoMock.Setup(r => r.GetByIdAsync(product.Id)).ReturnsAsync(product);
        _orderRepoMock.Setup(r => r.CreateOrderAsync(It.IsAny<Order>())).ReturnsAsync(order);

        var result = await _sut.CreateOrderFromCartAsync(userId, createDto);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task CreateOrderFromCartAsync_CartIsEmpty_ThrowsValidationException()
    {
        _cartRepoMock.Setup(r => r.GetCartByUserIdAsync(It.IsAny<string>())).ReturnsAsync(new Cart { Items = [] });

        await Assert.ThrowsAsync<ValidationException>(() => _sut.CreateOrderFromCartAsync("user1", new CreateOrderDto()));
    }

    [Fact]
    public async Task CreateOrderFromCartAsync_ProductNotFound_ThrowsProductNotFoundException()
    {
        var cartItem = new CartItem { ProductId = 99, Quantity = 1 };
        var cart = new Cart { Items = new List<CartItem> { cartItem } };

        _cartRepoMock.Setup(r => r.GetCartByUserIdAsync(It.IsAny<string>())).ReturnsAsync(cart);
        _productRepoMock.Setup(r => r.GetByIdAsync(cartItem.ProductId)).ReturnsAsync((Product?)null);

        await Assert.ThrowsAsync<ProductNotFoundException>(() => _sut.CreateOrderFromCartAsync("user1", new CreateOrderDto()));
    }

    [Fact]
    public async Task CreateOrderFromCartAsync_InsufficientStock_ThrowsInsufficientStockException()
    {
        var product = _fixture.Build<Product>().With(p => p.StockQuantity, 0).Create();
        var cartItem = new CartItem { ProductId = product.Id, Quantity = 1 };
        var cart = new Cart { Items = new List<CartItem> { cartItem } };

        _cartRepoMock.Setup(r => r.GetCartByUserIdAsync(It.IsAny<string>())).ReturnsAsync(cart);
        _productRepoMock.Setup(r => r.GetByIdAsync(product.Id)).ReturnsAsync(product);

        await Assert.ThrowsAsync<InsufficientStockException>(() => _sut.CreateOrderFromCartAsync("user1", new CreateOrderDto()));
    }

    [Fact]
    public async Task GetUserOrdersAsync_ReturnsSummaries()
    {
        var userId = "user1";
        var orders = _fixture.CreateMany<Order>(3).ToList();

        _orderRepoMock.Setup(r => r.GetOrdersByUserIdAsync(userId)).ReturnsAsync(orders);

        var result = await _sut.GetUserOrdersAsync(userId);

        Assert.Equal(orders.Count, result.Count());
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_InvalidTransition_ThrowsValidationException()
    {
        var order = _fixture.Build<Order>().With(o => o.UserId, "user1")
            .With(o => o.Status, OrderStatus.Delivered).Create();

        _orderRepoMock.Setup(r => r.GetByIdAsync(order.Id)).ReturnsAsync(order);

        await Assert.ThrowsAsync<ValidationException>(() => _sut.UpdateOrderStatusAsync(order.Id, OrderStatus.Pending, "user1"));
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_ValidStatusChange_ReturnsTrue()
    {
        var order = _fixture.Build<Order>().With(o => o.UserId, "user1")
            .With(o => o.Status, OrderStatus.Pending).Create();

        _orderRepoMock.Setup(r => r.GetByIdAsync(order.Id)).ReturnsAsync(order);
        _orderRepoMock.Setup(r => r.UpdateOrderStatusAsync(order.Id, OrderStatus.Processing)).ReturnsAsync(true);

        var result = await _sut.UpdateOrderStatusAsync(order.Id, OrderStatus.Processing, "user1");

        Assert.True(result);
    }
}

