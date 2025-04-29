using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Extensions;

public static class WebApplicationExtensions
{
    /// <summary>
    /// Applies any pending migrations for the context to the database.
    /// Creates the database if it does not already exist.
    /// </summary>
    /// <typeparam name="T">The DbContext type</typeparam>
    /// <param name="app">The web application</param>
    /// <param name="seeder">Optional seeder action</param>
    /// <returns>The web application</returns>
    public static WebApplication MigrateDatabase<T>(
        this WebApplication app, 
        Func<T, IServiceProvider, Task>? seeder = null) 
        where T : DbContext
    {
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<T>>();
            var context = services.GetService<T>();

            try
            {
                logger.LogInformation("Migrating database associated with context {DbContextName}", typeof(T).Name);

                if (context == null)
                {
                    logger.LogError("DbContext {DbContextName} not found in DI container", typeof(T).Name);
                    return app;
                }

                context.Database.Migrate();

                if (seeder != null)
                {
                    logger.LogInformation("Seeding database associated with context {DbContextName}", typeof(T).Name);
                    seeder(context, services).Wait();
                    logger.LogInformation("Seeded database associated with context {DbContextName}", typeof(T).Name);
                }

                logger.LogInformation("Migrated database associated with context {DbContextName}", typeof(T).Name);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, 
                    "An error occurred while migrating the database used on context {DbContextName}", typeof(T).Name);
            }
        }

        return app;
    }
}