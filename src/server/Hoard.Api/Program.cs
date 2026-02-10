using Hoard.Api.EventHandlers;
using Hoard.Api.Hubs;
using Hoard.Core.Application;
using Hoard.Core.Infrastructure;
using Hoard.Core.Services;
using Hoard.Messages.Performance;
using Hoard.Messages.Quotes;
using Rebus.Bus;
using Rebus.Config;

var builder = WebApplication.CreateBuilder(args);

builder.AddHoardConfiguration();

var sqlConnectionString = builder.Configuration.GetConnectionString("HoardDatabase")
                          ?? throw new InvalidOperationException("No SQL Server connection string configured.");
var rabbitConnectionString = builder.Configuration.GetConnectionString("RabbitMq")
                             ?? "amqp://guest:guest@localhost/";
var applicationInsightsConnectionString = builder.Configuration.GetConnectionString("ApplicationInsights")
                                          ?? throw new InvalidOperationException("No Application Insights connection string configured.");

// builder.Services.AddApplicationInsightsTelemetry(options =>
// {
//     options.ConnectionString = builder.Configuration.GetConnectionString("ApplicationInsights");
// });

builder.Services
    .AddHttpContextAccessor()
    .AddHoardData(sqlConnectionString)
    .AddHoardLogging(applicationInsightsConnectionString)
    .AutoRegisterHandlersFromAssemblyOf<PortfolioUpdatedSignalRHandler>()
    .AddHoardRebus(rabbitConnectionString, sqlConnectionString, sendOnly: false, "hoard.api")
    .AddHoardServices(builder.Configuration)
    .AddHoardApplication()
    .AddTelemetryInitializer("hoard.api")
    .Configure<RouteOptions>(options => options.LowercaseUrls = true);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddSignalR();

var app = builder.Build();
await app.ApplyMigrationsAndSeedAsync();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
    });
}
else
{
    app.UseHttpsRedirection();
}
app.UseAuthorization();
app.MapControllers();

await using (var scope = app.Services.CreateAsyncScope())
{
    var bus = scope.ServiceProvider.GetRequiredService<IBus>();
    await bus.Subscribe<PortfolioPerformanceCalculatedEvent>();
    await bus.Subscribe<QuoteChangedEvent>();
}

app.MapHub<PortfolioHub>(PortfolioHub.HubPath);
app.MapHub<InstrumentHub>(InstrumentHub.HubPath);

app.Run();