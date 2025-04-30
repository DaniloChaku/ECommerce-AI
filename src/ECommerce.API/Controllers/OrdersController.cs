using ECommerce.API.Extensions;
using ECommerce.BLL.Dtos.Orders;
using ECommerce.BLL.ServiceContracts;
using ECommerce.DAL.Constants;
using ECommerce.DAL.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[Authorize(Roles = EntityConstants.User.CustomerRole)]
public class OrdersController : BaseApiController
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<OrderSummaryDto>>> GetUserOrders()
    {
        var userId = User.GetCurrentUserId();
        var orders = await _orderService.GetUserOrdersAsync(userId);
        return Ok(orders);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDto>> GetOrder(int id)
    {
        var userId = User.GetCurrentUserId();

        try
        {
            var order = await _orderService.GetOrderByIdAsync(id, userId);
            return Ok(order);
        }
        catch (BaseApiException ex)
        {
            return StatusCode((int)ex.StatusCode, new { message = ex.Message });
        }
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<OrderDto>> CreateOrder(CreateOrderDto createOrderDto)
    {
        var userId = User.GetCurrentUserId();

        var order = await _orderService.CreateOrderFromCartAsync(userId, createOrderDto);
        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
    }
}