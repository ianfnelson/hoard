using Hoard.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Hoard.Core.Infrastructure;

public static class HostExtensions
{
    public static async Task ApplyMigrationsAndSeedAsync(this IHost app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<HoardContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<HoardContext>>();
        var seeder = scope.ServiceProvider.GetRequiredService<ReferenceDataSeeder>();

        logger.LogInformation("Applying EF migrations...");
        await db.Database.MigrateAsync();
        
        logger.LogInformation("Seeding reference data...");
        await seeder.SeedAsync();
    }
}