using Hoard.Core.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Hoard.Test.Unit.Core.Infrastructure;

public class AddHoardTelemetryTests
{
    private const string FakeConnectionString =
        "InstrumentationKey=00000000-0000-0000-0000-000000000000;" +
        "IngestionEndpoint=https://localhost/;" +
        "LiveEndpoint=https://localhost/";

    [Fact]
    public void RegistersOpenTelemetryProviders()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddHoardTelemetry("hoard.test", FakeConnectionString);

        using var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetService<TracerProvider>());
        Assert.NotNull(provider.GetService<MeterProvider>());
        Assert.NotNull(provider.GetService<LoggerProvider>());
    }

    [Fact]
    public void IsIdempotentAcrossMultipleCalls()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddHoardTelemetry("hoard.test", FakeConnectionString);
        services.AddHoardTelemetry("hoard.test", FakeConnectionString);

        using var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetService<TracerProvider>());
    }
}
