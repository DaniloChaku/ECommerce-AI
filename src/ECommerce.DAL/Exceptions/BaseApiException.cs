using System.Net;

namespace ECommerce.DAL.Exceptions;

public abstract class BaseApiException : Exception
{
    public HttpStatusCode StatusCode { get; }

    protected BaseApiException
        (string message,
        HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        : base(message)
    {
        StatusCode = statusCode;
    }
}