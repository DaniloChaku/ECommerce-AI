using ECommerce.BLL.Exceptions;
using ECommerce.DAL.Data.RepositoryContracts;
using ECommerce.DAL.Entities;
using ECommerce.DAL.Enums;
using Microsoft.EntityFrameworkCore;

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

    public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId)
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
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            _dbContext.Orders.Add(order);
            await _dbContext.SaveChangesAsync();

            await UpdateProductsQuantitesAsync(order);

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return order;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
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

    public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status)
    {
        var order = await _dbContext.Orders.FindAsync(orderId);

        if (order == null)
        {
            return false;
        }

        await UpdateOrderStatusAsync(orderId, status, order);

        await _dbContext.SaveChangesAsync();
        return true;
    }

    private async Task UpdateOrderStatusAsync(int orderId, OrderStatus status, Order order)
    {
        order.Status = status;

        if (status == OrderStatus.Cancelled && order.Status != OrderStatus.Delivered)
        {
            // If order is cancelled, restore product quantities
            var orderItems = await _dbContext.OrderItems
                .Where(oi => oi.OrderId == orderId)
                .ToListAsync();

            foreach (var item in orderItems)
            {
                var product = await _dbContext.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    product.StockQuantity += item.Quantity;
                }
            }
        }
    }
}