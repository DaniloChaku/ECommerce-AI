using System.Net;

namespace ECommerce.BLL.Exceptions;

public class UserAlreadyExistsException : BaseApiException
{
    public UserAlreadyExistsException(string email)
        : base($"User with email {email} already exists", HttpStatusCode.InternalServerError)
    {
    }
}