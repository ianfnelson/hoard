using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Hoard.Core.Data;

public class HoardContextFactory : IDesignTimeDbContextFactory<HoardContext>
{
    public HoardContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<HoardContext>();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddUserSecrets<HoardContextFactory>()
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("HoardDatabase");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string HoardDatabase not found.\nPlease define it before running migrations.");
        }

        optionsBuilder.UseSqlServer(connectionString);
        return new HoardContext(optionsBuilder.Options);
    }
}
