using Hoard.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rebus.Config;
using Rebus.Routing.TypeBased;

namespace Hoard.Core.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHoardData(this IServiceCollection services, string sqlConnStr)
    {
        services.AddDbContext<HoardContext>(options => options.UseSqlServer(sqlConnStr));
        services.AddTransient<ReferenceDataSeeder>();
        return services;
    }

    public static IServiceCollection AddHoardLogging(this IServiceCollection services)
    {
        services.AddLogging(cfg =>
        {
            cfg.ClearProviders();
            cfg.AddSimpleConsole(o =>
            {
                o.SingleLine = true;
                o.TimestampFormat = "HH:mm:ss ";
            });
        });
        return services;
    }

    public static IServiceCollection AddHoardRebus(this IServiceCollection services, string rabbitConnStr)
    {
        services.AddRebus(configure => configure
            .Logging(l => l.Console())
            .Transport(t => t.UseRabbitMq(rabbitConnStr, "hoard.bus"))
            .Routing(r => r.TypeBased().MapAssemblyOf<HoardContext>("hoard.input")));
        return services;
    }
}