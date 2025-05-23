﻿using ECommerce.DAL.Entities;
using ECommerce.DAL.Enums;

namespace ECommerce.DAL.Data.RepositoryContracts;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int id);
    Task<List<Order>> GetOrdersByUserIdAsync(string userId);
    Task<Order> CreateOrderAsync(Order order);
    Task<Order> UpdateOrderPaymentIntentAsync(int orderId, string paymentIntentId);
    Task<Order?> GetOrderByPaymentIntentIdAsync(string paymentIntentId);
    Task<Order> UpdateOrderStatusAsync(int orderId, OrderStatus status);
}