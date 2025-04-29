using AutoFixture;
using ECommerce.BLL.Dtos;
using ECommerce.BLL.Exceptions;
using ECommerce.BLL.ServiceContracts;
using ECommerce.BLL.Services;
using ECommerce.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace ECommerce.UnitTests.BLL.Services;

public class AuthServiceTests
{
    private readonly IFixture _fixture;
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _fixture = new Fixture();

        var store = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        var contextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        var userPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
        _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
            _userManagerMock.Object, contextAccessor.Object, userPrincipalFactory.Object, null!, null!, null!, null!);

        _jwtServiceMock = new Mock<IJwtService>();

        _sut = new AuthService(
            _userManagerMock.Object,
            _signInManagerMock.Object,
            _jwtServiceMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_UserAlreadyExists_ThrowsUserAlreadyExistsException()
    {
        // Arrange
        var request = _fixture.Create<RegisterRequest>();
        _userManagerMock.Setup(m => m.FindByEmailAsync(request.Email)).ReturnsAsync(new ApplicationUser());

        // Act & Assert
        await Assert.ThrowsAsync<UserAlreadyExistsException>(() => _sut.RegisterAsync(request));
    }

    [Fact]
    public async Task RegisterAsync_CreationFails_ThrowsValidationException()
    {
        // Arrange
        var request = _fixture.Create<RegisterRequest>();
        _userManagerMock.Setup(m => m.FindByEmailAsync(request.Email)).ReturnsAsync((ApplicationUser)null!);

        var identityResult = IdentityResult.Failed(new IdentityError { Description = "Invalid password" });
        _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), request.Password))
            .ReturnsAsync(identityResult);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _sut.RegisterAsync(request));
    }

    [Fact]
    public async Task RegisterAsync_ValidRequest_ReturnsAuthResponse()
    {
        // Arrange
        var request = _fixture.Create<RegisterRequest>();
        var token = _fixture.Create<string>();

        _userManagerMock.Setup(m => m.FindByEmailAsync(request.Email)).ReturnsAsync((ApplicationUser)null!);
        _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), request.Password))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(m => m.GetRolesAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(["Customer"]);
        _jwtServiceMock.Setup(j => j.GenerateToken(It.IsAny<ApplicationUser>(), It.IsAny<IList<string>>())).Returns(token);

        // Act
        var result = await _sut.RegisterAsync(request);

        // Assert
        Assert.Equal(token, result.Token);
    }

    [Fact]
    public async Task LoginAsync_InvalidUser_ThrowsInvalidCredentialsException()
    {
        // Arrange
        var request = _fixture.Create<LoginRequest>();
        _userManagerMock.Setup(m => m.FindByEmailAsync(request.Email)).ReturnsAsync((ApplicationUser)null!);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidCredentialsException>(() => _sut.LoginAsync(request));
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ThrowsInvalidCredentialsException()
    {
        // Arrange
        var request = _fixture.Create<LoginRequest>();
        var user = _fixture.Create<ApplicationUser>();

        _userManagerMock.Setup(m => m.FindByEmailAsync(request.Email)).ReturnsAsync(user);
        _signInManagerMock.Setup(m => m.CheckPasswordSignInAsync(user, request.Password, false))
            .ReturnsAsync(SignInResult.Failed);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidCredentialsException>(() => _sut.LoginAsync(request));
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsAuthResponse()
    {
        // Arrange
        var request = _fixture.Create<LoginRequest>();
        var user = _fixture.Create<ApplicationUser>();
        var token = _fixture.Create<string>();

        _userManagerMock.Setup(m => m.FindByEmailAsync(request.Email)).ReturnsAsync(user);
        _signInManagerMock.Setup(m => m.CheckPasswordSignInAsync(user, request.Password, false))
            .ReturnsAsync(SignInResult.Success);
        _userManagerMock.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(["Admin"]);
        _jwtServiceMock.Setup(j => j.GenerateToken(user, It.IsAny<IList<string>>())).Returns(token);

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        Assert.Equal(token, result.Token);
    }
}
