using Hangfire;
using Hoard.Core.Application;
using Hoard.Core.Infrastructure;
using Hoard.Scheduler;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.AddHoardConfiguration();

var sqlConnectionString = builder.Configuration.GetConnectionString("HoardDatabase")
                 ?? throw new InvalidOperationException("No connection string configured.");
var rabbitConnectionString = builder.Configuration.GetConnectionString("RabbitMq")
                    ?? "amqp://guest:guest@localhost/";

builder.Services.AddApplicationInsightsTelemetryWorkerService(options =>
{
    options.ConnectionString = builder.Configuration.GetConnectionString("ApplicationInsights");
});

builder.Services
    .AddHoardLogging(builder.Configuration)
    .AddHoardApplication()
    .AddHoardRebus(rabbitConnectionString, sendOnly: true, "hoard.scheduler")
    .AddTelemetryInitializer("hoard.scheduler");

builder.Services.AddHangfire(config =>
{
    config.UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(sqlConnectionString);
});
builder.Services.AddHangfireServer();
builder.Services.AddHostedService<SchedulerBootstrapper>();

var app = builder.Build();

app.UseHangfireDashboard("/jobs", new DashboardOptions
{
    Authorization = [new AllowAllDashboardAuthorizationFilter()]
});

app.MapGet("/", () => "Hoard Scheduler running.");
await app.RunAsync();