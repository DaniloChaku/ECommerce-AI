using System.Security.Claims;
using System.Security.Principal;
using AutoFixture;
using ECommerce.API.Controllers;
using ECommerce.BLL.Dtos.Orders;
using ECommerce.BLL.ServiceContracts;
using ECommerce.DAL.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ECommerce.UnitTests.API.Controllers;

public class OrdersControllerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IOrderService> _orderServiceMock;
    private readonly OrdersController _controller;

    public OrdersControllerTests()
    {
        _fixture = new Fixture();
        _orderServiceMock = new Mock<IOrderService>();
        _controller = new OrdersController(_orderServiceMock.Object);
    }

    private void SetFakeUser(string userId)
    {
        var identity = new GenericIdentity("testuser");
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId));

        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public async Task GetOrder_ReturnsOk_WithOrderDto_WhenFound()
    {
        var orderId = _fixture.Create<int>();
        var userId = _fixture.Create<string>();
        var orderDto = _fixture.Create<OrderDto>();

        SetFakeUser(userId);

        _orderServiceMock.Setup(s => s.GetOrderByIdAsync(orderId, userId)).ReturnsAsync(orderDto);

        var result = await _controller.GetOrder(orderId);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedOrder = Assert.IsType<OrderDto>(okResult.Value);
        Assert.Equal(orderDto.Id, returnedOrder.Id);
    }

    [Fact]
    public async Task GetOrder_ReturnsNotFound_WhenNotFoundExceptionThrown()
    {
        var orderId = _fixture.Create<int>();
        var userId = _fixture.Create<string>();
        var exception = new NotFoundException("Order not found");

        SetFakeUser(userId);

        _orderServiceMock.Setup(s => s.GetOrderByIdAsync(orderId, userId)).ThrowsAsync(exception);

        var result = await _controller.GetOrder(orderId);

        var objResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status404NotFound, objResult.StatusCode);
    }

    [Fact]
    public async Task GetOrder_ReturnsForbidden_WhenForbiddenExceptionThrown()
    {
        var orderId = _fixture.Create<int>();
        var userId = _fixture.Create<string>();
        var exception = new ForbiddenException("Forbidden access");

        SetFakeUser(userId);

        _orderServiceMock.Setup(s => s.GetOrderByIdAsync(orderId, userId)).ThrowsAsync(exception);

        var result = await _controller.GetOrder(orderId);

        var objResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status403Forbidden, objResult.StatusCode);
    }

    [Fact]
    public async Task GetUserOrders_ReturnsOk_WithListOfOrders()
    {
        var userId = _fixture.Create<string>();
        var orderSummaries = _fixture.CreateMany<OrderSummaryDto>(3).ToList();

        SetFakeUser(userId);

        _orderServiceMock.Setup(s => s.GetUserOrdersAsync(userId))
            .ReturnsAsync(orderSummaries);

        var result = await _controller.GetUserOrders();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var orders = Assert.IsAssignableFrom<IEnumerable<OrderSummaryDto>>(okResult.Value);
        Assert.Equal(3, ((List<OrderSummaryDto>)orders).Count);
    }

    [Fact]
    public async Task CreateOrder_ReturnsCreatedAtActionResult()
    {
        var userId = _fixture.Create<string>();
        var createDto = _fixture.Create<CreateOrderDto>();
        var createdOrder = _fixture.Create<OrderDto>();

        SetFakeUser(userId);

        _orderServiceMock.Setup(s => s.CreateOrderFromCartAsync(userId, createDto)).ReturnsAsync(createdOrder);

        var result = await _controller.CreateOrder(createDto);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returnedOrder = Assert.IsType<OrderDto>(createdResult.Value);
        Assert.Equal(createdOrder.Id, returnedOrder.Id);
    }
}
