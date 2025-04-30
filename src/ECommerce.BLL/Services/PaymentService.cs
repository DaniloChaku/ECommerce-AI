using ECommerce.BLL.Dtos.Orders;
using ECommerce.BLL.Dtos.Payments;
using ECommerce.BLL.Options;
using ECommerce.BLL.ServiceContracts;
using ECommerce.DAL.Data.RepositoryContracts;
using ECommerce.DAL.Entities;
using ECommerce.DAL.Enums;
using ECommerce.DAL.Exceptions;
using Microsoft.Extensions.Options;
using Stripe;

namespace ECommerce.BLL.Services;

public class PaymentService : IPaymentService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderService _orderService;
    private readonly StripeConfigOptions _stripeOptions;

    public PaymentService(
        IOrderRepository orderRepository,
        IOrderService orderService,
        IOptions<StripeConfigOptions> stripeOptions)
    {
        _orderRepository = orderRepository;
        _orderService = orderService;
        _stripeOptions = stripeOptions.Value;

        InitializeStripeConfiguration();
    }

    private void InitializeStripeConfiguration()
    {
        StripeConfiguration.ApiKey = _stripeOptions.ApiKey;
    }

    public async Task<PaymentIntentDto> CreatePaymentIntentAsync(int orderId, string userId)
    {
        var orderDto = await ValidateOrderForPaymentAsync(orderId, userId);
        var paymentIntent = await CreateStripePaymentIntentAsync(orderDto, orderId, userId);
        await UpdateOrderWithPaymentIntentAsync(orderId, paymentIntent.Id);

        return MapToPaymentIntentDto(paymentIntent, orderDto, orderId);
    }

    public async Task<OrderDto> ConfirmPaymentAsync(string paymentIntentId, string userId)
    {
        var paymentIntent = await GetStripePaymentIntentAsync(paymentIntentId);
        var order = await GetAndValidateOrderForPaymentConfirmationAsync(paymentIntentId, userId);

        if (IsPaymentSucceeded(paymentIntent))
        {
            order = await UpdateOrderToProcessingStatusAsync(order.Id);
        }

        return await _orderService.GetOrderByIdAsync(order.Id, userId);
    }

    public async Task<PaymentStatusDto> GetPaymentStatusAsync(string paymentIntentId, string userId)
    {
        var paymentIntent = await GetStripePaymentIntentAsync(paymentIntentId);
        var order = await GetAndValidateOrderForPaymentConfirmationAsync(paymentIntentId, userId);

        return CreatePaymentStatusDto(paymentIntent, order);
    }

    #region Private Helper Methods

    private async Task<OrderDto> ValidateOrderForPaymentAsync(int orderId, string userId)
    {
        var orderDto = await _orderService.GetOrderByIdAsync(orderId, userId);

        if (orderDto == null)
        {
            throw new NotFoundException($"Order with ID {orderId} not found");
        }

        if (orderDto.Status != OrderStatus.Pending.ToString())
        {
            throw new ValidationException($"Order with ID {orderId} is not in pending status");
        }

        if (orderDto.UserId != userId)
        {
            throw new ForbiddenException("Unauthorized access to order");
        }

        return orderDto;
    }

    private async Task<PaymentIntent> CreateStripePaymentIntentAsync(OrderDto orderDto, int orderId, string userId)
    {
        var amountInSmallestUnit = ConvertToSmallestCurrencyUnit(orderDto.TotalAmount);
        var paymentIntentOptions = CreatePaymentIntentOptions(amountInSmallestUnit, orderId, userId);

        var paymentIntentService = new PaymentIntentService();
        return await paymentIntentService.CreateAsync(paymentIntentOptions);
    }

    private static long ConvertToSmallestCurrencyUnit(decimal amount)
    {
        return Convert.ToInt64(amount * 100);
    }

    private PaymentIntentCreateOptions CreatePaymentIntentOptions(long amount, int orderId, string userId)
    {
        return new PaymentIntentCreateOptions
        {
            Amount = amount,
            Currency = _stripeOptions.Currency,
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
            {
                Enabled = true,
            },
            Metadata = new Dictionary<string, string>
            {
                { "OrderId", orderId.ToString() },
                { "UserId", userId }
            }
        };
    }

    private async Task UpdateOrderWithPaymentIntentAsync(int orderId, string paymentIntentId)
    {
        await _orderRepository.UpdateOrderPaymentIntentAsync(orderId, paymentIntentId);
    }

    private PaymentIntentDto MapToPaymentIntentDto(PaymentIntent paymentIntent, OrderDto orderDto, int orderId)
    {
        return new PaymentIntentDto
        {
            ClientSecret = paymentIntent.ClientSecret,
            PaymentIntentId = paymentIntent.Id,
            Amount = orderDto.TotalAmount,
            Currency = _stripeOptions.Currency,
            OrderId = orderId
        };
    }

    private static async Task<PaymentIntent> GetStripePaymentIntentAsync(string paymentIntentId)
    {
        var paymentIntentService = new PaymentIntentService();
        return await paymentIntentService.GetAsync(paymentIntentId);
    }

    private async Task<Order> GetAndValidateOrderForPaymentConfirmationAsync(string paymentIntentId, string userId)
    {
        var order = await _orderRepository.GetOrderByPaymentIntentIdAsync(paymentIntentId);

        if (order == null)
        {
            throw new NotFoundException($"Order with payment intent ID {paymentIntentId} not found");
        }

        if (order.UserId != userId)
        {
            throw new ForbiddenException("Unauthorized access to order");
        }

        return order;
    }

    private static bool IsPaymentSucceeded(PaymentIntent paymentIntent)
    {
        return paymentIntent.Status == "succeeded";
    }

    private async Task<Order> UpdateOrderToProcessingStatusAsync(int orderId)
    {
        return await _orderRepository.UpdateOrderStatusAsync(orderId, OrderStatus.Processing);
    }

    private PaymentStatusDto CreatePaymentStatusDto(PaymentIntent paymentIntent, Order order)
    {
        return new PaymentStatusDto
        {
            Status = paymentIntent.Status,
            OrderId = order.Id,
            Amount = order.TotalAmount,
            PaidAt = order.PaidAt
        };
    }

    #endregion
}