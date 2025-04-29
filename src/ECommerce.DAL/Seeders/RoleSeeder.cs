using ECommerce.DAL.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.DAL.Seeders;

public static class RoleSeeder
{
    public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        if (!await roleManager.RoleExistsAsync(EntityConstants.User.CustomerRole))
        {
            await roleManager.CreateAsync(new IdentityRole(EntityConstants.User.CustomerRole));
        }
    }
}
