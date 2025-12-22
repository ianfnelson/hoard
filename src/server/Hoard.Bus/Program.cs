using Hoard.Bus.Handlers.Valuations;
using Hoard.Core.Application;
using Hoard.Core.Infrastructure;
using Hoard.Core.Services;
using Hoard.Messages.Holdings;
using Hoard.Messages.Performance;
using Hoard.Messages.Positions;
using Hoard.Messages.Prices;
using Hoard.Messages.Quotes;
using Hoard.Messages.Snapshots;
using Hoard.Messages.Valuations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rebus.Bus;
using Rebus.Config;

var builder = Host.CreateApplicationBuilder(args);

builder.AddHoardConfiguration();

var sqlConnectionString = builder.Configuration.GetConnectionString("HoardDatabase")
                          ?? throw new InvalidOperationException("No SQL Server connection string configured.");
var rabbitConnectionString = builder.Configuration.GetConnectionString("RabbitMq")
                             ?? "amqp://guest:guest@localhost/";
var applicationInsightsConnectionString = builder.Configuration.GetConnectionString("ApplicationInsights")
                                          ?? throw new InvalidOperationException("No Application Insights connection string configured.");

// builder.Services.AddApplicationInsightsTelemetryWorkerService(options =>
// {
//     options.ConnectionString = builder.Configuration.GetConnectionString("ApplicationInsights");
// });

builder.Services
    .AddHoardData(sqlConnectionString)
    .AddHoardLogging(applicationInsightsConnectionString)
    .AutoRegisterHandlersFromAssemblyOf<CalculateValuationsSaga>()
    .AddHoardRebus(rabbitConnectionString, sendOnly: false, "hoard.bus")
    .AddHoardServices()
    .AddHoardApplication()
    .AddTelemetryInitializer("hoard.bus");

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var bus = scope.ServiceProvider.GetRequiredService<IBus>();
    await bus.Subscribe<HoldingsCalculatedEvent>();
    await bus.Subscribe<HoldingChangedEvent>();
    await bus.Subscribe<HoldingsBackfilledEvent>();
    
    await bus.Subscribe<PositionPerformanceCalculatedEvent>();
    await bus.Subscribe<PortfolioPerformanceCalculatedEvent>();
    await bus.Subscribe<PerformanceCalculatedEvent>();
    await bus.Subscribe<PortfolioPerformancesInvalidatedEvent>();
    
    await bus.Subscribe<PriceChangedEvent>();
    await bus.Subscribe<PriceRefreshedEvent>();
    await bus.Subscribe<PricesRefreshedEvent>();
    
    await bus.Subscribe<PositionsCalculatedEvent>();
    
    await bus.Subscribe<QuoteChangedEvent>();
    
    await bus.Subscribe<HoldingValuationsCalculatedEvent>();
    await bus.Subscribe<HoldingValuationsChangedEvent>();
    await bus.Subscribe<PortfolioValuationCalculatedEvent>();
    await bus.Subscribe<PortfolioValuationChangedEvent>();
    await bus.Subscribe<PortfolioValuationsInvalidatedEvent>();
    await bus.Subscribe<ValuationsCalculatedEvent>();
    await bus.Subscribe<ValuationsBackfilledEvent>();
    
    await bus.Subscribe<SnapshotCalculatedEvent>();
    await bus.Subscribe<SnapshotsCalculatedEvent>();
}

await app.RunAsync();
