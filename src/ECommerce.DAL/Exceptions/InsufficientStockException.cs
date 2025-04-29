namespace ECommerce.DAL.Exceptions;

public class InsufficientStockException : BaseApiException
{
    public InsufficientStockException(int productId, int availableQuantity, int requestedQuantity)
        : base(
            $"Not enough items in stock for product {productId}. Available: {availableQuantity}, Requested: {requestedQuantity}")
    {
    }
}
