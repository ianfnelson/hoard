using Hoard.Core.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.AddHoardConfiguration();

var sqlConnectionString = builder.Configuration.GetConnectionString("HoardDatabase")
                 ?? throw new InvalidOperationException("No connection string configured.");
var rabbitConnectionString = builder.Configuration.GetConnectionString("RabbitMq")
                    ?? "amqp://guest:guest@localhost/";

builder.Services
    .AddHoardData(sqlConnectionString)
    .AddHoardLogging()
    .AddHoardRebus(rabbitConnectionString, sendOnly:false, "hoard.bus");

var app = builder.Build();
await app.ApplyMigrationsAndSeedAsync();
await app.RunAsync();