using System.Net;

namespace ECommerce.BLL.Exceptions;

public class ProductNotFoundException : BaseApiException
{
    public ProductNotFoundException(int productId)
        : base($"Product with ID {productId} not found.", HttpStatusCode.NotFound)
    {
    }
}