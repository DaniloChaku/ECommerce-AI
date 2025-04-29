using ECommerce.DAL.Enums;

namespace ECommerce.BLL.Helpers;

public static class OrderTransitionHelper
{
    public static bool IsValidStatusTransition(OrderStatus currentStatus, OrderStatus newStatus)
    {
        return (currentStatus, newStatus) switch
        {
            (OrderStatus.Pending, OrderStatus.Processing) => true,
            (OrderStatus.Pending, OrderStatus.Cancelled) => true,

            (OrderStatus.Processing, OrderStatus.Shipped) => true,
            (OrderStatus.Processing, OrderStatus.Cancelled) => true,

            (OrderStatus.Shipped, OrderStatus.Delivered) => true,

            (OrderStatus.Delivered, _) => false,
            (OrderStatus.Cancelled, _) => false,

            var (current, next) when current == next => true,

            _ => false
        };
    }
}
