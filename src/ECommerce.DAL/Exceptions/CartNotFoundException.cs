using System.Net;

namespace ECommerce.DAL.Exceptions;

public class CartNotFoundException : BaseApiException
{
    public CartNotFoundException(string userId)
        : base($"Cart not found for user {userId}.", HttpStatusCode.NotFound)
    {
    }
}
