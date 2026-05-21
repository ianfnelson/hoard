using Hoard.Api.EventHandlers;
using Hoard.Api.Hubs;
using Hoard.Core.Application;
using Hoard.Core.Domain.Calculators;
using Hoard.Core.Infrastructure;
using Hoard.Core.Infrastructure.Integrations;
using Hoard.Messages.News;
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

builder.Services
    .AddHttpContextAccessor()
    .AddHoardData(sqlConnectionString)
    .AutoRegisterHandlersFromAssemblyOf<PortfolioUpdatedSignalRHandler>()
    .AddHoardRebus(rabbitConnectionString, sqlConnectionString, sendOnly: false, "hoard.api")
    .AddIntegrations(builder.Configuration)
    .AddHoardApplication()
    .AddHoardCalculators()
    .Configure<RouteOptions>(options => options.LowercaseUrls = true);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddSignalR();

var app = builder.Build();
await app.ApplyMigrationsAndSeedAsync();

app.UsePathBase("/api");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("../openapi/v1.json", "v1");
    });
}

if (!app.Environment.IsDevelopment())
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
    await bus.Subscribe<NewsArticleCreatedEvent>();
}

app.MapHub<PortfolioHub>(PortfolioHub.HubPath);
app.MapHub<InstrumentHub>(InstrumentHub.HubPath);

app.Run();