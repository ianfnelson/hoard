using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Hoard.Core.Infrastructure;

public static class HostBuilderExtensions
{
    public static TBuilder AddHoardConfiguration<TBuilder>(this TBuilder builder)
    where TBuilder : IHostApplicationBuilder
    {
        builder.Configuration
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
            .AddUserSecrets(typeof(HostBuilderExtensions).Assembly, optional: true)
            .AddEnvironmentVariables();

        return builder;
    }
}