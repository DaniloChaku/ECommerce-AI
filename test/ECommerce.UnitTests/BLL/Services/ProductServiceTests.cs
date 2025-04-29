using AutoFixture;
using ECommerce.BLL.ServiceContracts;
using ECommerce.DAL.Common;
using ECommerce.DAL.Data.RepositoryContracts;
using ECommerce.DAL.Entities;
using Moq;

namespace ECommerce.UnitTests.BLL.Services;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Fixture _fixture;
    private readonly ProductService _sut;

    public ProductServiceTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors
        .OfType<ThrowingRecursionBehavior>()
        .ToList()
        .ForEach(b => _fixture.Behaviors.Remove(b));

        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _productRepositoryMock = new Mock<IProductRepository>();
        _sut = new ProductService(_productRepositoryMock.Object);
    }

    [Fact]
    public async Task GetProductByIdAsync_ProductExists_ReturnsMappedDto()
    {
        // Arrange
        var product = _fixture.Build<Product>()
                              .With(p => p.Category, _fixture.Create<Category>())
                              .Create();
        _productRepositoryMock.Setup(repo => repo.GetByIdAsync(product.Id)).ReturnsAsync(product);

        // Act
        var result = await _sut.GetProductByIdAsync(product.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(product.Id, result.Id);
        Assert.Equal(product.Name, result.Name);
        Assert.Equal(product.Category.Name, result.CategoryName);
    }

    [Fact]
    public async Task GetProductByIdAsync_ProductDoesNotExist_ReturnsNull()
    {
        // Arrange
        _productRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Product?)null);

        // Act
        var result = await _sut.GetProductByIdAsync(1);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SearchProductsAsync_ValidSearchParams_ReturnsMappedPagedResult()
    {
        // Arrange
        var searchParams = _fixture.Create<ProductSearchParams>();
        var products = _fixture.Build<Product>()
                               .With(p => p.Category, _fixture.Create<Category>())
                               .CreateMany(3)
                               .ToList();

        var pagedResult = new PagedResult<Product>
        {
            Items = products,
            TotalCount = 30,
            PageNumber = 2,
            PageSize = 10
        };

        _productRepositoryMock.Setup(r => r.SearchProductsAsync(searchParams)).ReturnsAsync(pagedResult);

        // Act
        var result = await _sut.SearchProductsAsync(searchParams);

        // Assert
        Assert.Equal(pagedResult.TotalCount, result.TotalCount);
        Assert.Equal(pagedResult.PageSize, result.PageSize);
        Assert.Equal(pagedResult.PageNumber, result.PageNumber);
        Assert.Equal(products.Count, result.Items.Count);
        Assert.Equal(products[0].Id, result.Items[0].Id);
    }

    [Fact]
    public async Task GetAllCategoriesAsync_CategoriesExist_ReturnsMappedDtos()
    {
        // Arrange
        var categories = _fixture.CreateMany<Category>(3).ToList();
        _productRepositoryMock.Setup(r => r.GetAllCategoriesAsync()).ReturnsAsync(categories);

        // Act
        var result = (await _sut.GetAllCategoriesAsync()).ToList();

        // Assert
        Assert.Equal(categories.Count, result.Count);
        for (int i = 0; i < categories.Count; i++)
        {
            Assert.Equal(categories[i].Id, result[i].Id);
            Assert.Equal(categories[i].Name, result[i].Name);
        }
    }

    [Fact]
    public async Task GetCategoryByIdAsync_CategoryExists_ReturnsMappedDto()
    {
        // Arrange
        var category = _fixture.Create<Category>();
        _productRepositoryMock.Setup(r => r.GetCategoryByIdAsync(category.Id)).ReturnsAsync(category);

        // Act
        var result = await _sut.GetCategoryByIdAsync(category.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(category.Id, result.Id);
        Assert.Equal(category.Name, result.Name);
    }

    [Fact]
    public async Task GetCategoryByIdAsync_CategoryDoesNotExist_ReturnsNull()
    {
        // Arrange
        _productRepositoryMock.Setup(r => r.GetCategoryByIdAsync(It.IsAny<int>())).ReturnsAsync((Category?)null);

        // Act
        var result = await _sut.GetCategoryByIdAsync(99);

        // Assert
        Assert.Null(result);
    }
}