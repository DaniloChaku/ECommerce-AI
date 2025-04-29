using ECommerce.DAL.Entities;

namespace ECommerce.BLL.ServiceContracts;

public interface IJwtService
{
    string GenerateToken(ApplicationUser user, IList<string> roles);
}
