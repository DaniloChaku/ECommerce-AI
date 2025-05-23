﻿using ECommerce.BLL.Dtos.Orders;
using ECommerce.DAL.Enums;

namespace ECommerce.BLL.ServiceContracts;

public interface IOrderService
{
    Task<OrderDto> GetOrderByIdAsync(int id, string userId);
    Task<List<OrderSummaryDto>> GetUserOrdersAsync(string userId);
    Task<OrderDto> CreateOrderFromCartAsync(string userId, CreateOrderDto createOrderDto);
}