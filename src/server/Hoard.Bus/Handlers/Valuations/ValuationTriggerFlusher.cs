using Hoard.Core.Application;
using Hoard.Core.Application.Valuations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Hoard.Bus.Handlers.Valuations;

public sealed class ValuationTriggerFlusher : BackgroundService
{
    private readonly IValuationTriggerBuffer _buffer;
    private readonly ILogger<ValuationTriggerFlusher> _logger;
    private readonly TimeSpan _interval;
    private readonly IServiceScopeFactory _scopeFactory;

    public ValuationTriggerFlusher(
        IServiceScopeFactory scopeFactory,
        IValuationTriggerBuffer buffer,
        ILogger<ValuationTriggerFlusher> logger,
        IConfiguration config)
    {
        _scopeFactory = scopeFactory;
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
                
                using var scope = _scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                
                foreach (var date in dates)
                {
                    var command = new TriggerCalculateValuationsCommand(Guid.NewGuid(), date);
                    await mediator.SendAsync(command);
                }
            }
        }
        catch (OperationCanceledException) { /* graceful shutdown */ }
    }
}