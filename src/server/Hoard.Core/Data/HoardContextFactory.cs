using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Hoard.Core.Data;

public class HoardContextFactory : IDesignTimeDbContextFactory<HoardContext>
{
    public HoardContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<HoardContext>();

        // Try to read from environment variable
        var connectionString = Environment.GetEnvironmentVariable("HOARD_CONNSTR");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Environment variable 'HOARD_CONNSTR' not set. " +
                "Please define it before running migrations, e.g.:\n" +
                "  export HOARD_CONNSTR=\"Server=localhost;Database=Hoard;User Id=sa;Password=...;TrustServerCertificate=True;\"");
        }

        optionsBuilder.UseSqlServer(connectionString);
        return new HoardContext(optionsBuilder.Options);
    }
}