using ECommerce.BLL.Dtos;

namespace ECommerce.BLL.ServiceContracts;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
}