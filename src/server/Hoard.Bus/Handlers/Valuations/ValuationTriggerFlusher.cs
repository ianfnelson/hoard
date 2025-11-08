using Hoard.Core.Messages.Valuations;
using Microsoft.Extensions.Configuration;
using Rebus.Bus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Hoard.Bus.Handlers.Valuations;

public sealed class ValuationTriggerFlusher : BackgroundService
{
    private readonly IBus _bus;
    private readonly IValuationTriggerBuffer _buffer;
    private readonly ILogger<ValuationTriggerFlusher> _logger;
    private readonly TimeSpan _interval;

    public ValuationTriggerFlusher(
        IBus bus,
        IValuationTriggerBuffer buffer,
        ILogger<ValuationTriggerFlusher> logger,
        IConfiguration config)
    {
        _bus = bus;
        _buffer = buffer;
        _logger = logger;
        _interval = TimeSpan.FromSeconds(config.GetValue("Valuations:FlushSeconds", 15));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var timer = new PeriodicTimer(_interval);
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                var dates = _buffer.SnapshotAndClear();
                if (dates.Length == 0) continue;

                _logger.LogInformation("Flushing valuations for {Count} dates", dates.Length);

                foreach (var date in dates)
                {
                    await _bus.SendLocal(new CalculateValuationsCommand(Guid.NewGuid())
                    {
                        AsOfDate = date
                    });
                }
            }
        }
        catch (OperationCanceledException) { /* graceful shutdown */ }
    }
}