using Hoard.Core.Data;
using Hoard.Core.Messages;
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

    public static IServiceCollection AddHoardRebus(this IServiceCollection services, string rabbitConnectionString, bool sendOnly)
    {
        services.AddRebus(configure =>
        {
            var config = sendOnly ?
                configure.Transport(t => t.UseRabbitMqAsOneWayClient(rabbitConnectionString)) :
                configure.Transport(t => t.UseRabbitMq(rabbitConnectionString, "hoard.bus"));

            if (!sendOnly)
            {
                config.Options(o =>
                {
                    o.SetMaxParallelism(8);
                    o.SetNumberOfWorkers(1);
                });
            }

            config.Routing(r => r.TypeBased().MapAssemblyNamespaceOf<RecalculateHoldingsCommand>("hoard.bus"));

            return config;
        });
        return services;
    }
}