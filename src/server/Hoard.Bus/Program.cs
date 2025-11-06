using Hoard.Bus.Handlers.Holdings;
using Hoard.Core.Infrastructure;
using Hoard.Core.Messages.Holdings;
using Hoard.Core.Messages.Prices;
using Hoard.Core.Messages.Quotes;
using Hoard.Core.Services;
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

builder.Services
    .AddHoardData(sqlConnectionString)
    .AddHoardLogging()
    .AutoRegisterHandlersFromAssemblyOf<BackfillHoldingsSaga>()
    .AddHoardRebus(rabbitConnectionString, sendOnly: false, "hoard.bus")
    .AddHoardServices();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var bus = scope.ServiceProvider.GetRequiredService<IBus>();
    await bus.Subscribe<HoldingsCalculatedEvent>();
    await bus.Subscribe<PriceRefreshedEvent>();
    await bus.Subscribe<QuoteRefreshedEvent>();
}

await app.RunAsync();
