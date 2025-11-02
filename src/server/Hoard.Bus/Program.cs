using Hoard.Bus.Handlers;
using Hoard.Core.Infrastructure;
using Hoard.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
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
    .AutoRegisterHandlersFromAssemblyNamespaceOf<RecalculateHoldingsCommandHandler>()
    .AddHoardRebus(rabbitConnectionString, sendOnly: false, "hoard.bus")
    .AddHoardServices();

var app = builder.Build();
await app.ApplyMigrationsAndSeedAsync();
await app.RunAsync();