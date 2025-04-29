namespace ECommerce.DAL.Exceptions;

public class CartNotFoundException : NotFoundException
{
    public CartNotFoundException(string userId)
        : base($"Cart not found for user {userId}.")
    {
    }
}
