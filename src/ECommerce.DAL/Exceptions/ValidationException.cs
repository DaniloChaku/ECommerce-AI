namespace ECommerce.DAL.Exceptions;

public class ValidationException : BaseApiException
{
    public ValidationException(string message)
        : base(message)
    {
    }
}