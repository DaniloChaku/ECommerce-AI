using ECommerce.DAL.Common;

namespace ECommerce.UnitTests.DAL.Common;

public class PagedResultTests
{
    [Fact]
    public void TotalPages_TotalCountDivisibleByPageSize_ReturnsCorrectPages()
    {
        // Arrange
        var pagedResult = new PagedResult<string>
        {
            TotalCount = 100,
            PageSize = 10
        };

        // Act
        var totalPages = pagedResult.TotalPages;

        // Assert
        Assert.Equal(10, totalPages);
    }

    [Fact]
    public void TotalPages_TotalCountNotDivisibleByPageSize_RoundsUp()
    {
        // Arrange
        var pagedResult = new PagedResult<string>
        {
            TotalCount = 95,
            PageSize = 10
        };

        // Act
        var totalPages = pagedResult.TotalPages;

        // Assert
        Assert.Equal(10, totalPages);
    }

    [Fact]
    public void TotalPages_ZeroTotalCount_ReturnsZero()
    {
        // Arrange
        var pagedResult = new PagedResult<string>
        {
            TotalCount = 0,
            PageSize = 10
        };

        // Act
        var totalPages = pagedResult.TotalPages;

        // Assert
        Assert.Equal(0, totalPages);
    }

    [Fact]
    public void Items_InitializedWithEmptyList_ReturnsEmpty()
    {
        // Arrange
        var pagedResult = new PagedResult<string>();

        // Act
        var items = pagedResult.Items;

        // Assert
        Assert.NotNull(items);
        Assert.Empty(items);
    }
}