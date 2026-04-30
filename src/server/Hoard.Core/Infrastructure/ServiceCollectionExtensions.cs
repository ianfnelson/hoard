using Azure.Monitor.OpenTelemetry.AspNetCore;
using Hoard.Core.Data;
using Hoard.Messages.Holdings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Rebus.Config;
using Rebus.OpenTelemetry.Configuration;
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

        public IServiceCollection AddHoardTelemetry(
            string serviceName,
            string applicationInsightsConnectionString,
            bool instrumentSql = true)
        {
            services.AddOpenTelemetry()
                .ConfigureResource(r => r.AddService(serviceName))
                .UseAzureMonitor(o => o.ConnectionString = applicationInsightsConnectionString)
                .WithTracing(t =>
                {
                    if (instrumentSql)
                    {
                        t.AddSqlClientInstrumentation();
                    }
                    t.AddRebusInstrumentation();
                })
                .WithMetrics(m => m.AddRebusInstrumentation());

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
    }
}
