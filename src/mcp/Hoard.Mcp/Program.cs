using Hoard.Mcp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using ModelContextProtocol;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.SetMinimumLevel(LogLevel.Warning);
builder.Logging.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Warning);

var baseUrl = Environment.GetEnvironmentVariable("HOARD_API_BASE_URL") ?? "http://hoard.iannelson.uk/api";

builder.Services.AddHttpClient<HoardApiClient>(client =>
{
    client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddStandardResilienceHandler(options =>
{
    // Total budget for a single API call, including retries.
    options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(30);
    // Per-attempt timeout must be shorter than the total budget.
    options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);
    // Circuit breaker sampling window must be at least 2x the attempt timeout.
    options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(20);
    // Don't retry mutating methods (POST/PATCH/PUT/DELETE): a lost response after
    // the server has already processed the request would otherwise duplicate the
    // side effect — e.g. creating a transaction twice. GETs remain retryable.
    options.Retry.DisableForUnsafeHttpMethods();
});

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

var app = builder.Build();
await app.RunAsync();
