using System.Net;

namespace ECommerce.BLL.Exceptions;

public class InvalidCredentialsException : BaseApiException
{
    public InvalidCredentialsException()
        : base("Invalid email or password", HttpStatusCode.Unauthorized)
    {
    }
}