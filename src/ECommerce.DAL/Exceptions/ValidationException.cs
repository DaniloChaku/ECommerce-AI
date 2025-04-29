namespace ECommerce.BLL.Exceptions;

public class ValidationException : BaseApiException
{
    public ValidationException(string message)
        : base(message)
    {
    }
}