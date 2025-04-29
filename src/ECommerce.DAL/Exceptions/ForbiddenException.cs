using System.Net;

namespace ECommerce.DAL.Exceptions;

public class ForbiddenException : BaseApiException
{
    public ForbiddenException(string message) 
        : base(message, HttpStatusCode.Forbidden)
    {
    }
}
