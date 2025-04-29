using System.Net;

namespace ECommerce.DAL.Exceptions;

public class CartItemNotFoundException : BaseApiException
{
    public CartItemNotFoundException(int itemId, int cartId)
        : base($"Cart item with ID {itemId} not found in cart {cartId}.", HttpStatusCode.NotFound)
    {
    }
}