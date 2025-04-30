using ECommerce.DAL.Exceptions;
using ECommerce.DAL.Data.RepositoryContracts;
using ECommerce.DAL.Entities;
using ECommerce.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace ECommerce.DAL.Data.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _dbContext;

    public OrderRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        return await _dbContext.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<List<Order>> GetOrdersByUserIdAsync(string userId)
    {
        return await _dbContext.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<Order> CreateOrderAsync(Order order)
    {
        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync();

        await UpdateProductsQuantitesAsync(order);

        await _dbContext.SaveChangesAsync();

        return order;
    }

    private async Task UpdateProductsQuantitesAsync(Order order)
    {
        foreach (var item in order.Items)
        {
            var product = await _dbContext.Products.FindAsync(item.ProductId);

            if (product == null)
            {
                throw new ProductNotFoundException(item.ProductId);
            }

            if (product.StockQuantity < item.Quantity)
            {
                throw new InsufficientStockException(
                    product.Id,
                    product.StockQuantity,
                    item.Quantity);
            }

            product.StockQuantity -= item.Quantity;
        }
    }

    public async Task<Order> UpdateOrderPaymentIntentAsync(int orderId, string paymentIntentId)
    {
        var order = await _dbContext.Orders.FindAsync(orderId);

        ValidateOrderExists(orderId, order);

        order.PaymentIntentId = paymentIntentId;
        await _dbContext.SaveChangesAsync();

        return order;
    }

    private static void ValidateOrderExists(int orderId, [NotNull] Order? order)
    {
        if (order == null)
        {
            throw new NotFoundException($"Order with ID {orderId} not found");
        }
    }

    public async Task<Order?> GetOrderByPaymentIntentIdAsync(string paymentIntentId)
    {
        return await _dbContext.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.PaymentIntentId == paymentIntentId);
    }

    public async Task<Order> UpdateOrderStatusAsync(int orderId, OrderStatus status)
    {
        var order = await _dbContext.Orders.FindAsync(orderId);

        ValidateOrderExists(orderId, order);

        order.Status = status;

        if (status == OrderStatus.Processing)
        {
            order.PaidAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync();

        return order;
    }
}