using Hoard.Core.Application;
using Hoard.Core.Infrastructure;
using Hoard.Core.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddHoardConfiguration();

var sqlConnectionString = builder.Configuration.GetConnectionString("HoardDatabase")
                          ?? throw new InvalidOperationException("No connection string configured.");
var rabbitConnectionString = builder.Configuration.GetConnectionString("RabbitMq")
                             ?? "amqp://guest:guest@localhost/";

builder.Services
    .AddHoardData(sqlConnectionString)
    .AddHoardLogging()
    .AddHoardRebus(rabbitConnectionString, sendOnly: true, "hoard.api")
    .AddHoardServices()
    .AddHoardApplication()
    .Configure<RouteOptions>(options => options.LowercaseUrls = true);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

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

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();