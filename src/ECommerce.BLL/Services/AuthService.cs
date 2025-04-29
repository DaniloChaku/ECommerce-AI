using ECommerce.BLL.Dtos;
using ECommerce.BLL.Exceptions;
using ECommerce.BLL.ServiceContracts;
using ECommerce.DAL.Constants;
using ECommerce.DAL.Entities;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.BLL.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtService _jwtService;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IJwtService jwtService)
    {
        _userManager = userManager;
        _jwtService = jwtService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        await ValidateUserExistenceAsync(request);
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            Name = request.Name
        };

        await CreateUserAsync(request, user);
        return await GetJwtAsync(user!);
    }

    private async Task CreateUserAsync(RegisterRequest request, ApplicationUser user)
    {
        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join("\n", result.Errors.Select(e => e.Description));
            throw new ValidationException(errors);
        }

        await _userManager.AddToRoleAsync(user, EntityConstants.User.CustomerRole);
    }

    private async Task ValidateUserExistenceAsync(RegisterRequest request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new UserAlreadyExistsException(request.Email);
        }
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        await ValidateCredentialsAsync(request, user);

        return await GetJwtAsync(user!);
    }

    private async Task<AuthResponse> GetJwtAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user!);

        var token = _jwtService.GenerateToken(user!, roles);

        return new AuthResponse { Token = token };
    }

    private async Task ValidateCredentialsAsync(LoginRequest request, ApplicationUser? user)
    {
        if (user == null)
        {
            throw new InvalidCredentialsException();
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
        {
            throw new InvalidCredentialsException();
        }
    }
}