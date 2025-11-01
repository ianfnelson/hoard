using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Hoard.Core.Infrastructure;

public static class HostBuilderExtensions
{
    public static void AddHoardConfiguration(this HostApplicationBuilder builder)
    {
        builder.Configuration
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
            .AddUserSecrets(typeof(HostBuilderExtensions).Assembly, optional: true)
            .AddEnvironmentVariables();
    }
}