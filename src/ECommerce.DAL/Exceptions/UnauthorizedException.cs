using System.Net;

namespace ECommerce.DAL.Exceptions;

public class UnauthorizedException : BaseApiException
{
    public UnauthorizedException(string message = "Unauthorized access")
        : base(message, HttpStatusCode.Unauthorized)
    {
    }
}