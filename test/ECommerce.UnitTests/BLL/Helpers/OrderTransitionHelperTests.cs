using ECommerce.BLL.Helpers;
using ECommerce.DAL.Enums;

namespace ECommerce.UnitTests.BLL.Helpers;

public class OrderTransitionHelperTests
{
    [Theory]
    [InlineData(OrderStatus.Pending, OrderStatus.Processing, true)]
    [InlineData(OrderStatus.Pending, OrderStatus.Cancelled, true)]
    [InlineData(OrderStatus.Processing, OrderStatus.Shipped, true)]
    [InlineData(OrderStatus.Processing, OrderStatus.Cancelled, true)]
    [InlineData(OrderStatus.Shipped, OrderStatus.Delivered, true)]
    [InlineData(OrderStatus.Pending, OrderStatus.Pending, true)]
    [InlineData(OrderStatus.Processing, OrderStatus.Processing, true)]
    [InlineData(OrderStatus.Shipped, OrderStatus.Shipped, true)]
    [InlineData(OrderStatus.Delivered, OrderStatus.Delivered, false)]
    [InlineData(OrderStatus.Delivered, OrderStatus.Shipped, false)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Processing, false)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Shipped, false)]
    [InlineData(OrderStatus.Shipped, OrderStatus.Cancelled, false)]
    public void IsValidStatusTransition_VariousTransitions_ReturnsExpectedResult(
        OrderStatus current, OrderStatus next, bool expected)
    {
        // Act
        var result = OrderTransitionHelper.IsValidStatusTransition(current, next);

        // Assert
        Assert.Equal(expected, result);
    }
}