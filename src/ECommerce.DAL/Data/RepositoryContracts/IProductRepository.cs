using System.ComponentModel.DataAnnotations;
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
    [StringLength(100, ErrorMessage = "SearchTerm cannot exceed 100 characters.")]
    public string? SearchTerm { get; set; }

    public int? CategoryId { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "MinPrice must be a non-negative number.")]
    public decimal? MinPrice { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "MaxPrice must be a non-negative number.")]
    public decimal? MaxPrice { get; set; }

    [RegularExpression("^(name|price)?$", ErrorMessage = "SortBy must be either 'name', 'price', or empty.")]
    public string? SortBy { get; set; }

    public bool SortDescending { get; set; } = false;

    [Range(1, int.MaxValue, ErrorMessage = "PageNumber must be at least 1.")]
    public int PageNumber { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100.")]
    public int PageSize { get; set; } = 10;
}