using ECommerce.DAL.Constants;
using ECommerce.DAL.Seeders;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace ECommerce.UnitTests.DAL.Seeders;

public class RoleSeederTests
{
    private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;

    public RoleSeederTests()
    {
        var roleStoreMock = new Mock<IRoleStore<IdentityRole>>();
        _roleManagerMock = new Mock<RoleManager<IdentityRole>>(
            roleStoreMock.Object,
            null!, null!, null!, null!);

        _serviceProviderMock = new Mock<IServiceProvider>();
        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(RoleManager<IdentityRole>)))
            .Returns(_roleManagerMock.Object);
    }

    [Fact]
    public async Task SeedRolesAsync_RoleDoesNotExist_CreatesRole()
    {
        // Arrange
        _roleManagerMock.Setup(r => r.RoleExistsAsync(EntityConstants.User.CustomerRole))
                        .ReturnsAsync(false);

        _roleManagerMock.Setup(r => r.CreateAsync(
            It.Is<IdentityRole>(role => role.Name == EntityConstants.User.CustomerRole)))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        await RoleSeeder.SeedRolesAsync(_serviceProviderMock.Object);

        // Assert
        _roleManagerMock.Verify(r => r.RoleExistsAsync(EntityConstants.User.CustomerRole), Times.Once);
        _roleManagerMock.Verify(r => r.CreateAsync(
            It.Is<IdentityRole>(role => role.Name == EntityConstants.User.CustomerRole)), Times.Once);
    }

    [Fact]
    public async Task SeedRolesAsync_RoleAlreadyExists_DoesNotCreateRole()
    {
        // Arrange
        _roleManagerMock.Setup(r => r.RoleExistsAsync(EntityConstants.User.CustomerRole))
                        .ReturnsAsync(true);

        // Act
        await RoleSeeder.SeedRolesAsync(_serviceProviderMock.Object);

        // Assert
        _roleManagerMock.Verify(r => r.RoleExistsAsync(EntityConstants.User.CustomerRole), Times.Once);
        _roleManagerMock.Verify(r => r.CreateAsync(It.IsAny<IdentityRole>()), Times.Never);
    }
}