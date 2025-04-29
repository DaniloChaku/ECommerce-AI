using ECommerce.API.Controllers;
using ECommerce.BLL.Dtos;
using ECommerce.BLL.ServiceContracts;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ECommerce.UnitTests.API.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _controller = new AuthController(_authServiceMock.Object);
    }

    [Fact]
    public async Task Register_ValidRequest_ReturnsOkResultWithToken()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            Name = "Test User",
            Email = "test@example.com",
            Password = "P@ssword123",
            ConfirmPassword = "P@ssword123"
        };

        var expectedResponse = new AuthResponse { Token = "test_token" };

        _authServiceMock
            .Setup(s => s.RegisterAsync(registerRequest))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Register(registerRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var authResponse = Assert.IsType<AuthResponse>(okResult.Value);
        Assert.Equal(expectedResponse.Token, authResponse.Token);
    }

    [Fact]
    public async Task Login_ValidRequest_ReturnsOkResultWithToken()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "test@example.com",
            Password = "P@ssword123"
        };

        var expectedResponse = new AuthResponse { Token = "login_token" };

        _authServiceMock
            .Setup(s => s.LoginAsync(loginRequest))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var authResponse = Assert.IsType<AuthResponse>(okResult.Value);
        Assert.Equal(expectedResponse.Token, authResponse.Token);
    }
}