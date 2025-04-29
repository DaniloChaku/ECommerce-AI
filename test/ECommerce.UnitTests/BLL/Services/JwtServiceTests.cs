using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ECommerce.BLL.Options;
using ECommerce.BLL.Services;
using ECommerce.DAL.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ECommerce.UnitTests.BLL.Services;

public class JwtServiceTests
{
    private readonly JwtOptions _jwtOptions;
    private readonly JwtService _sut;

    public JwtServiceTests()
    {
        _jwtOptions = new JwtOptions
        {
            Secret = "ThisIsASecretKeyForTestingPurposesOnly12334567881234547678232445!",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpirationMinutes = 60
        };

        var options = Options.Create(_jwtOptions);
        _sut = new JwtService(options);
    }

    [Fact]
    public void GenerateToken_ValidUserAndRoles_ReturnsValidJwt()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = "123",
            UserName = "testuser",
            Email = "test@example.com",
            Name = "Test User"
        };

        var roles = new List<string> { "Admin", "User" };

        // Act
        var token = _sut.GenerateToken(user, roles);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = _jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Secret)),
            ValidateLifetime = false
        };

        handler.ValidateToken(token, tokenValidationParameters, out var validatedToken);

        var jwtToken = Assert.IsType<JwtSecurityToken>(validatedToken);
        Assert.Equal("testuser", jwtToken.Claims.First(c => c.Type == ClaimTypes.Name).Value);
        Assert.Contains(jwtToken.Claims, c => c.Type == ClaimTypes.Role && c.Value == "Admin");
        Assert.Contains(jwtToken.Claims, c => c.Type == ClaimTypes.Role && c.Value == "User");
        Assert.Equal(_jwtOptions.Issuer, jwtToken.Issuer);
        Assert.Equal(_jwtOptions.Audience, jwtToken.Audiences.First());
    }

    [Fact]
    public void GenerateToken_NullOrEmptyRoles_ReturnsTokenWithNoRoles()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = "456",
            UserName = "noroletest",
            Email = "no@roles.com",
            Name = "No Role"
        };

        var roles = new List<string>();

        // Act
        var token = _sut.GenerateToken(user, roles);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        Assert.DoesNotContain(jwtToken.Claims, c => c.Type == ClaimTypes.Role);
    }
}
