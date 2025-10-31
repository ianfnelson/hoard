using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("HoardDatabase");

builder.Services.AddHangfire(config => config
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(connectionString));

builder.Services.AddHangfireServer();

var app = builder.Build();

app.UseHangfireDashboard("/jobs");

app.MapGet("/", () => "Hoard Scheduler running.");

RecurringJob.AddOrUpdate(
    "refresh-quotes",
    () => Console.WriteLine("Refreshing quotes..."),
    Cron.Minutely);

app.Run();