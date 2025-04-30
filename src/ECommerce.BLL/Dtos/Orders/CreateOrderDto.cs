using System.ComponentModel.DataAnnotations;
using ECommerce.DAL.Constants;

namespace ECommerce.BLL.Dtos.Orders;

public class CreateOrderDto
{
    [Length(EntityConstants.Order.AddressMinLength, EntityConstants.Order.AddressMaxLength)]
    public string ShippingAddress { get; set; } = string.Empty;
}