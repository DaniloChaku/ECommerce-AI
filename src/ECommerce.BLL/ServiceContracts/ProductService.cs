using ECommerce.BLL.Dtos;
using ECommerce.DAL.Common;
using ECommerce.DAL.Data.RepositoryContracts;
using ECommerce.DAL.Entities;

namespace ECommerce.BLL.ServiceContracts;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        return product != null ? MapProductToDto(product) : null;
    }

    public async Task<PagedResult<ProductDto>> SearchProductsAsync(ProductSearchParams searchParams)
    {
        var result = await _productRepository.SearchProductsAsync(searchParams);

        var mappedResult = new PagedResult<ProductDto>
        {
            Items = result.Items.ConvertAll(MapProductToDto),
            TotalCount = result.TotalCount,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize
        };

        return mappedResult;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
    {
        var categories = await _productRepository.GetAllCategoriesAsync();
        return categories.Select(MapCategoryToDto);
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
    {
        var category = await _productRepository.GetCategoryByIdAsync(id);
        return category != null ? MapCategoryToDto(category) : null;
    }

    private static ProductDto MapProductToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name ?? string.Empty,
            StockQuantity = product.StockQuantity,
            ImageUrl = product.ImageUrl
        };
    }

    private static CategoryDto MapCategoryToDto(Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        };
    }
}