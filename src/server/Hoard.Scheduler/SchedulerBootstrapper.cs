using Hangfire;
using Hoard.Core;
using Hoard.Core.Messages.Holdings;
using Hoard.Core.Messages.Prices;
using Hoard.Core.Messages.Quotes;
using Hoard.Core.Messages.Valuations;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace Hoard.Scheduler;

public class SchedulerBootstrapper : IHostedService
{
    private readonly IBackgroundJobClient _jobs;
    private readonly IRecurringJobManager _recurring;
    private readonly IBus _bus;
    private readonly ILogger<SchedulerBootstrapper> _logger;

    public SchedulerBootstrapper(
        IBackgroundJobClient jobs,
        IRecurringJobManager recurring,
        IBus bus,
        ILogger<SchedulerBootstrapper> logger)
    {
        _jobs = jobs;
        _recurring = recurring;
        _bus = bus;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Registering Hoard Scheduler jobs...");

        RegisterCalculateHoldings();
        RegisterRefreshQuotes();
        RegisterRefreshPrices();
        RegisterCalculateValuations();
        
        return Task.CompletedTask;
    }

    private void RegisterCalculateHoldings()
    {
        _recurring.AddOrUpdate(
            "calculate-holdings",
            () => SendCalculateHoldingsCommand(),
            "0 2 * * *" // nightly at 2am
        );
    }
    
    private void RegisterRefreshQuotes()
    {
        _recurring.AddOrUpdate(
            "refresh-quotes",
            () => SendRefreshQuotesCommand(),
            "*/2 8-17 * * 1-5" // every two minutes during UK trading hours
        );
    }

    private void RegisterRefreshPrices()
    {
        _recurring.AddOrUpdate(
            "refresh-prices",
            () => SendRefreshPricesCommand(),
            "40 21 * * *" // daily at 21:40
        );
    }
    
    private void RegisterCalculateValuations()
    {
        _recurring.AddOrUpdate(
            "calculate-valuations",
            () => SendCalculateValuationsCommand(),
            "0 23 * * *" // daily at 23:00
        );
    }
    
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Scheduler stopping...");
        return Task.CompletedTask;
    }

    // ReSharper disable once MemberCanBePrivate.Global public required for Hangfire background jobs
    public async Task SendCalculateHoldingsCommand()
    {
        _logger.LogInformation("Enqueuing CalculateHoldingsCommand...");

        var command = new CalculateHoldingsCommand(Guid.NewGuid())
        {
            AsOfDate = DateOnlyHelper.TodayLocal()
        };
        
        await _bus.Send(command);
    }

    // ReSharper disable once MemberCanBePrivate.Global public required for Hangfire background jobs
    public async Task SendRefreshQuotesCommand()
    {
        _logger.LogInformation("Enqueuing RefreshQuotesCommand...");
        await _bus.Send(new RefreshQuotesCommand(Guid.NewGuid()));
    }

    // ReSharper disable once MemberCanBePrivate.Global public required for Hangfire background jobs
    public async Task SendRefreshPricesCommand()
    {
        _logger.LogInformation("Enqueuing RefreshPricesCommand...");

        var command = new RefreshPricesCommand(Guid.NewGuid())
        {
            AsOfDate = DateOnlyHelper.TodayLocal()
        };
        
        await _bus.Send(command);
    }
    
    // ReSharper disable once MemberCanBePrivate.Global public required for Hangfire background jobs
    public async Task SendCalculateValuationsCommand()
    {
        _logger.LogInformation("Enqueuing CalculateValuationsCommand...");

        var command = new CalculateValuationsCommand(Guid.NewGuid())
        {
            AsOfDate = DateOnlyHelper.TodayLocal()
        };
        
        await _bus.Send(command);
    }
}