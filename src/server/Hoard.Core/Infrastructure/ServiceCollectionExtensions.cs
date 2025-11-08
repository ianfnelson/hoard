using Hoard.Core.Data;
using Hoard.Core.Messages.Holdings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rebus.Config;
using Rebus.Persistence.InMem;
using Rebus.Retry.Simple;
using Rebus.Routing.TypeBased;
using Rebus.Sagas;
using Rebus.Sagas.Exclusive;

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

    public static IServiceCollection AddHoardRebus(
        this IServiceCollection services, 
        string rabbitConnectionString, 
        bool sendOnly, 
        string connectionName)
    {
        services.AddRebus(configure =>
        {
            var config = sendOnly ?
                configure.Transport(t => t
                    .UseRabbitMqAsOneWayClient(rabbitConnectionString)
                    .ClientConnectionName(connectionName)) :
                configure.Transport(t => t
                    .UseRabbitMq(rabbitConnectionString, "hoard.bus")
                    .ClientConnectionName(connectionName));
            
            if (!sendOnly)
            {
                config.Sagas(x =>
                {
                    x.StoreInMemory();
                    x.EnforceExclusiveAccess();
                });
                config.Timeouts(x => x.StoreInMemory());
                
                config.Options(o =>
                {
                    o.SetMaxParallelism(32);
                    o.SetNumberOfWorkers(8);

                    o.RetryStrategy(
                        "hoard.error",
                        maxDeliveryAttempts: 5, 
                        secondLevelRetriesEnabled: true);
                });
            }

            config.Routing(r => r.TypeBased().MapAssemblyOf<CalculateHoldingsCommand>("hoard.bus"));
            return config;
        });
        return services;
    }
}