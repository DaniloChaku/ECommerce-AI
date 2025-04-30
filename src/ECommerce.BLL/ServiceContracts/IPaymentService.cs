using ECommerce.BLL.Dtos.Orders;
using ECommerce.BLL.Dtos.Payments;

namespace ECommerce.BLL.ServiceContracts;

public interface IPaymentService
{
    Task<PaymentIntentDto> CreatePaymentIntentAsync(int orderId, string userId);
    Task<OrderDto> ConfirmPaymentAsync(string paymentIntentId, string userId);
    Task<PaymentStatusDto> GetPaymentStatusAsync(string paymentIntentId, string userId);
}