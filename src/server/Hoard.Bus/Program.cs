using Hoard.Bus.Handlers.Holdings;
using Hoard.Bus.Handlers.Valuations;
using Hoard.Core.Application;
using Hoard.Core.Infrastructure;
using Hoard.Core.Services;
using Hoard.Messages.Holdings;
using Hoard.Messages.Prices;
using Hoard.Messages.Quotes;
using Hoard.Messages.Valuations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rebus.Bus;
using Rebus.Config;

var builder = Host.CreateApplicationBuilder(args);

builder.AddHoardConfiguration();

var sqlConnectionString = builder.Configuration.GetConnectionString("HoardDatabase")
                 ?? throw new InvalidOperationException("No connection string configured.");
var rabbitConnectionString = builder.Configuration.GetConnectionString("RabbitMq")
                    ?? "amqp://guest:guest@localhost/";

builder.Services.AddSingleton<IValuationTriggerBuffer, ValuationTriggerBuffer>();
builder.Services.AddHostedService<ValuationTriggerFlusher>();

builder.Services.AddSingleton<IHoldingTriggerBuffer, HoldingTriggerBuffer>();
builder.Services.AddHostedService<HoldingTriggerFlusher>();

builder.Services
    .AddHoardData(sqlConnectionString)
    .AddHoardLogging()
    .AutoRegisterHandlersFromAssemblyOf<CalculateValuationsSaga>()
    .AddHoardRebus(rabbitConnectionString, sendOnly: false, "hoard.bus")
    .AddHoardServices()
    .AddHoardApplication();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var bus = scope.ServiceProvider.GetRequiredService<IBus>();
    await bus.Subscribe<HoldingsCalculatedEvent>();
    
    await bus.Subscribe<PriceChangedEvent>();
    await bus.Subscribe<PriceRefreshedEvent>();
    
    await bus.Subscribe<QuoteChangedEvent>();
    
    await bus.Subscribe<HoldingValuationCalculatedEvent>();
    await bus.Subscribe<ValuationsCalculatedEvent>();
}

await app.RunAsync();
