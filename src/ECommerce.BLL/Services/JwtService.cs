using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ECommerce.BLL.Options;
using ECommerce.BLL.ServiceContracts;
using ECommerce.DAL.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ECommerce.BLL.Services;

public class JwtService : IJwtService
{
    private readonly JwtOptions _jwtConfigs;

    public JwtService(IOptions<JwtOptions> configuration)
    {
        _jwtConfigs = configuration.Value;
    }

    public string GenerateToken(ApplicationUser user, IList<string> roles)
    {
        List<Claim> claims = GetCalims(user, roles);

        return GetToken(claims);
    }

    private string GetToken(List<Claim> claims)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfigs.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.Now.AddMinutes(_jwtConfigs.ExpirationMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwtConfigs.Issuer,
            audience: _jwtConfigs.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static List<Claim> GetCalims(ApplicationUser user, IList<string> roles)
    {
        var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim("name", user.Name)
            };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        return claims;
    }
}
