using System.Net;

namespace ECommerce.DAL.Exceptions;

public class NotFoundException : BaseApiException
{
    public NotFoundException(string message) 
        : base(message, HttpStatusCode.NotFound)
    {
    }
}
