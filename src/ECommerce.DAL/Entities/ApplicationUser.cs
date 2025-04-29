using Microsoft.AspNetCore.Identity;

namespace ECommerce.DAL.Entities;

public class ApplicationUser : IdentityUser
{
    public string Name { get; set; } = string.Empty;
}