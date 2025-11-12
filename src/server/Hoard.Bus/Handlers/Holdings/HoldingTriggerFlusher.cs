using Hoard.Core.Application;
using Hoard.Core.Application.Holdings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Hoard.Bus.Handlers.Holdings;

public class HoldingTriggerFlusher : BackgroundService
{
    private readonly IHoldingTriggerBuffer _buffer;
    private readonly ILogger<HoldingTriggerFlusher> _logger;
    private readonly TimeSpan _interval;
    private readonly IServiceScopeFactory _scopeFactory;

    public HoldingTriggerFlusher(
        IServiceScopeFactory scopeFactory,
        IHoldingTriggerBuffer buffer,
        ILogger<HoldingTriggerFlusher> logger,
        IConfiguration config)
    {
        _scopeFactory = scopeFactory;
        _buffer = buffer;
        _logger = logger;
        _interval = TimeSpan.FromSeconds(config.GetValue("Holdings:FlushSeconds", 5));
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

                _logger.LogInformation("Flushing holdings for {Count} dates", dates.Length);
                
                using var scope = _scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                
                foreach (var date in dates)
                {
                    var command = new TriggerCalculateHoldingsCommand(Guid.NewGuid(), date);
                    await mediator.SendAsync(command);
                }
            }
        }
        catch (OperationCanceledException) { /* graceful shutdown */ }
    }
}