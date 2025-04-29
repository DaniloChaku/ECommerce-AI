using ECommerce.DAL.Common;
using ECommerce.DAL.Entities;

namespace ECommerce.DAL.Data.RepositoryContracts;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id);
    Task<PagedResult<Product>> SearchProductsAsync(ProductSearchParams searchParams);
    Task<List<Category>> GetAllCategoriesAsync();
    Task<Category?> GetCategoryByIdAsync(int id);
}

public class ProductSearchParams
{
    public string? SearchTerm { get; set; }
    public int? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = false;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}