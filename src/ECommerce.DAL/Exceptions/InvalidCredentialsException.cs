using System.Net;

namespace ECommerce.DAL.Exceptions;

public class InvalidCredentialsException : BaseApiException
{
    public InvalidCredentialsException()
        : base("Invalid email or password", HttpStatusCode.Unauthorized)
    {
    }
}