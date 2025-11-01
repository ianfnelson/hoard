using Hoard.Core.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.AddHoardConfiguration();

var sqlConnStr = builder.Configuration.GetConnectionString("HoardDatabase")
                 ?? throw new InvalidOperationException("No connection string configured.");
var rabbitConnStr = builder.Configuration.GetConnectionString("RabbitMq")
                    ?? "amqp://guest:guest@localhost/";

builder.Services
    .AddHoardData(sqlConnStr)
    .AddHoardLogging()
    .AddHoardRebus(rabbitConnStr);

var app = builder.Build();
await app.ApplyMigrationsAndSeedAsync();
await app.RunAsync();