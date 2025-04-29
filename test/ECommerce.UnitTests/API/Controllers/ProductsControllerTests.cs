using AutoFixture;
using ECommerce.API.Controllers;
using ECommerce.BLL.Dtos;
using ECommerce.BLL.ServiceContracts;
using ECommerce.DAL.Common;
using ECommerce.DAL.Data.RepositoryContracts;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ECommerce.UnitTests.API.Controllers;

public class ProductsControllerTests
{
    private readonly Mock<IProductService> _productServiceMock;
    private readonly Fixture _fixture;
    private readonly ProductsController _sut;

    public ProductsControllerTests()
    {
        _fixture = new Fixture();

        _productServiceMock = new Mock<IProductService>();
        _sut = new ProductsController(_productServiceMock.Object);
    }

    [Fact]
    public async Task GetProducts_ValidSearchParams_ReturnsOkWithPagedResult()
    {
        // Arrange
        var searchParams = new ProductSearchParams()
        {
            MinPrice = 10,
            MaxPrice = 20,
        };
        var pagedResult = _fixture.Create<PagedResult<ProductDto>>();
        _productServiceMock.Setup(s => s.SearchProductsAsync(searchParams)).ReturnsAsync(pagedResult);

        // Act
        var result = await _sut.GetProducts(searchParams);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedValue = Assert.IsAssignableFrom<PagedResult<ProductDto>>(okResult.Value);
        Assert.Equal(pagedResult.TotalCount, returnedValue.TotalCount);
    }

    [Fact]
    public async Task GetProduct_ProductExists_ReturnsOkWithProduct()
    {
        // Arrange
        var product = _fixture.Create<ProductDto>();
        _productServiceMock.Setup(s => s.GetProductByIdAsync(product.Id)).ReturnsAsync(product);

        // Act
        var result = await _sut.GetProduct(product.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedProduct = Assert.IsAssignableFrom<ProductDto>(okResult.Value);
        Assert.Equal(product.Id, returnedProduct.Id);
    }

    [Fact]
    public async Task GetProduct_ProductDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        _productServiceMock.Setup(s => s.GetProductByIdAsync(It.IsAny<int>())).ReturnsAsync((ProductDto?)null);

        // Act
        var result = await _sut.GetProduct(999);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetCategories_ReturnsOkWithCategories()
    {
        // Arrange
        var categories = _fixture.CreateMany<CategoryDto>(3).ToList();
        _productServiceMock.Setup(s => s.GetAllCategoriesAsync()).ReturnsAsync(categories);

        // Act
        var result = await _sut.GetCategories();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedCategories = Assert.IsAssignableFrom<IEnumerable<CategoryDto>>(okResult.Value);
        Assert.Equal(3, returnedCategories.Count());
    }

    [Fact]
    public async Task GetCategory_CategoryExists_ReturnsOkWithCategory()
    {
        // Arrange
        var category = _fixture.Create<CategoryDto>();
        _productServiceMock.Setup(s => s.GetCategoryByIdAsync(category.Id)).ReturnsAsync(category);

        // Act
        var result = await _sut.GetCategory(category.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedCategory = Assert.IsAssignableFrom<CategoryDto>(okResult.Value);
        Assert.Equal(category.Id, returnedCategory.Id);
    }

    [Fact]
    public async Task GetCategory_CategoryDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        _productServiceMock.Setup(s => s.GetCategoryByIdAsync(It.IsAny<int>())).ReturnsAsync((CategoryDto?)null);

        // Act
        var result = await _sut.GetCategory(888);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }
}
