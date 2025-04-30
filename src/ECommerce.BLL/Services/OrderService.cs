using ECommerce.BLL.Dtos.Orders;
using ECommerce.BLL.Helpers;
using ECommerce.BLL.ServiceContracts;
using ECommerce.DAL.Data.RepositoryContracts;
using ECommerce.DAL.Entities;
using ECommerce.DAL.Enums;
using ECommerce.DAL.Exceptions;

namespace ECommerce.BLL.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;

    public OrderService(
        IOrderRepository orderRepository,
        ICartRepository cartRepository,
        IProductRepository productRepository)
    {
        _orderRepository = orderRepository;
        _cartRepository = cartRepository;
        _productRepository = productRepository;
    }

    public async Task<OrderDto> GetOrderByIdAsync(int id, string userId)
    {
        var order = await _orderRepository.GetByIdAsync(id);

        ValidateOrderExists(id, order);
        ValidateOrderOwner(userId, order!);

        return MapOrderToDto(order!);
    }

    private static void ValidateOrderOwner(string userId, Order order)
    {
        if (order.UserId != userId)
        {
            throw new ForbiddenException("Unauthorized access to order");
        }
    }

    private static void ValidateOrderExists(int id, Order? order)
    {
        if (order == null)
        {
            throw new NotFoundException($"Order with ID {id} not found");
        }
    }

    public async Task<List<OrderSummaryDto>> GetUserOrdersAsync(string userId)
    {
        var orders = await _orderRepository.GetOrdersByUserIdAsync(userId);
        return orders.ConvertAll(MapOrderToSummaryDto);
    }

    public async Task<OrderDto> CreateOrderFromCartAsync(string userId, CreateOrderDto createOrderDto)
    {
        var cart = await _cartRepository.GetCartByUserIdAsync(userId);

        ValidateCartNotEmpty(cart);

        var order = new Order
        {
            UserId = userId,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            ShippingAddress = createOrderDto.ShippingAddress,
            Items = []
        };

        await SetOrderPriceAsync(cart!, order);

        var createdOrder = await _orderRepository.CreateOrderAsync(order);

        await _cartRepository.ClearCartAsync(cart!);

        return MapOrderToDto(createdOrder);
    }

    private async Task SetOrderPriceAsync(Cart cart, Order order)
    {
        decimal totalAmount = 0;

        foreach (var cartItem in cart.Items)
        {
            var product = await _productRepository.GetByIdAsync(cartItem.ProductId);

            ValidateOrderExists(cartItem, product);
            ValidateProductStock(cartItem, product!);

            var orderItem = new OrderItem
            {
                ProductId = product!.Id,
                Product = product,
                Quantity = cartItem.Quantity,
                UnitPrice = product.Price
            };

            order.Items.Add(orderItem);
            totalAmount += (orderItem.UnitPrice * orderItem.Quantity);
        }

        order.TotalAmount = totalAmount;

        static void ValidateOrderExists(CartItem cartItem, Product? product)
        {
            if (product == null)
            {
                throw new ProductNotFoundException(cartItem.ProductId);
            }
        }

        static void ValidateProductStock(CartItem cartItem, Product product)
        {
            if (product.StockQuantity < cartItem.Quantity)
            {
                throw new InsufficientStockException(
                    product.Id,
                    product.StockQuantity,
                    cartItem.Quantity);
            }
        }
    }

    private static void ValidateCartNotEmpty(Cart? cart)
    {
        if (cart == null || !cart.Items.Any())
        {
            throw new ValidationException("Cannot create order from empty cart");
        }
    }

    private static OrderDto MapOrderToDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            UserId = order.UserId,
            OrderDate = order.OrderDate,
            Status = order.Status.ToString(),
            TotalAmount = order.TotalAmount,
            ShippingAddress = order.ShippingAddress,
            PaidAt = order.PaidAt,
            Items = order.Items.Select(item => new OrderItemDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                ProductName = item.Product?.Name ?? string.Empty,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            }).ToList()
        };
    }

    private static OrderSummaryDto MapOrderToSummaryDto(Order order)
    {
        return new OrderSummaryDto
        {
            Id = order.Id,
            OrderDate = order.OrderDate,
            Status = order.Status.ToString(),
            TotalAmount = order.TotalAmount,
            ItemCount = order.Items.Count
        };
    }
}