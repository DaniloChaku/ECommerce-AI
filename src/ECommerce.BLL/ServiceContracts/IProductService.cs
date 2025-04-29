using ECommerce.BLL.Dtos;
using ECommerce.DAL.Common;
using ECommerce.DAL.Data.RepositoryContracts;

namespace ECommerce.BLL.ServiceContracts;

public interface IProductService
{
    Task<ProductDto?> GetProductByIdAsync(int id);
    Task<PagedResult<ProductDto>> SearchProductsAsync(ProductSearchParams searchParams);
    Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
    Task<CategoryDto?> GetCategoryByIdAsync(int id);
}