﻿using ECommerce.API.Extensions;
using ECommerce.BLL.Dtos;
using ECommerce.BLL.ServiceContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[Authorize]
public class CartController : BaseApiController
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CartDto>> GetCart()
    {
        var userId = User.GetCurrentUserId();
        var cart = await _cartService.GetCartAsync(userId);
        return Ok(cart);
    }

    [HttpPost("items")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CartDto>> AddItemToCart(AddCartItemDto itemDto)
    {
        var userId = User.GetCurrentUserId();
        var cart = await _cartService.AddItemToCartAsync(userId, itemDto);
        return Ok(cart);
    }

    [HttpPut("items/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CartDto>> UpdateCartItem(int id, UpdateCartItemDto itemDto)
    {
        var userId = User.GetCurrentUserId();
        var cart = await _cartService.UpdateCartItemAsync(userId, id, itemDto);
        return Ok(cart);
    }

    [HttpDelete("items/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CartDto>> RemoveCartItem(int id)
    {
        var userId = User.GetCurrentUserId();
        var cart = await _cartService.RemoveCartItemAsync(userId, id);
        return Ok(cart);
    }

    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CartDto>> ClearCart()
    {
        var userId = User.GetCurrentUserId();
        var cart = await _cartService.ClearCartAsync(userId);
        return Ok(cart);
    }
}