using Hoard.Core.Data;
using Hoard.Messages.Holdings;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rebus.Config;
using Rebus.Persistence.InMem;
using Rebus.Retry.Simple;
using Rebus.Routing.TypeBased;
using Rebus.Sagas.Exclusive;

namespace Hoard.Core.Infrastructure;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddHoardData(string sqlConnStr)
        {
            services.AddDbContext<HoardContext>(options => options.UseSqlServer(sqlConnStr));
            services.AddTransient<ReferenceDataSeeder>();
            return services;
        }

        public IServiceCollection AddHoardLogging(string applicationInsightsConnectionString)
        {
            services.AddLogging(cfg =>
            {
                cfg.ClearProviders();
                cfg.AddSimpleConsole(o =>
                {
                    o.SingleLine = true;
                    o.TimestampFormat = "HH:mm:ss ";
                });
                cfg.AddApplicationInsights(
                    configureTelemetryConfiguration: tc =>
                    {
                        tc.ConnectionString = applicationInsightsConnectionString;
                    },
                    configureApplicationInsightsLoggerOptions: _ => { });
            });
            return services;
        }

        public IServiceCollection AddHoardRebus(
            string rabbitConnectionString,
            string sqlConnectionString,
            bool sendOnly,
            string queueName)
        {
            services.AddRebus(configure =>
            {
                var config = sendOnly ?
                    configure.Transport(t => t
                        .UseRabbitMqAsOneWayClient(rabbitConnectionString)
                        .SetConnectionName(queueName)) :
                    configure.Transport(t => t
                        .UseRabbitMq(rabbitConnectionString, queueName)
                        .SetConnectionName(queueName));
            
                if (!sendOnly)
                {
                    config.Sagas(x =>
                    {
                        x.StoreInSqlServer(sqlConnectionString, "__RebusSagas", "__RebusSagaIndex");
                        
                        x.EnforceExclusiveAccess();
                    });
                    config.Timeouts(x => x.StoreInSqlServer(sqlConnectionString, "__RebusTimeouts"));
                
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

                config.Routing(r => r.TypeBased().MapAssemblyOf<CalculateHoldingsBusCommand>("hoard.bus"));
                return config;
            });
            return services;
        }

        public IServiceCollection AddTelemetryInitializer(string roleName)
        {
            services.AddSingleton<ITelemetryInitializer>(sp =>
            {
                var initializer = new RoleNameInitializer(roleName);
                return initializer;
            });
        
            return services;
        }
    }
}