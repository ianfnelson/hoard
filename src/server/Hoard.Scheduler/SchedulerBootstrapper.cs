using Hangfire;
using Hoard.Core;
using Hoard.Messages.Holdings;
using Hoard.Messages.Prices;
using Hoard.Messages.Quotes;
using Hoard.Messages.Valuations;
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
            "9 17,22,23 * * *" // daily at 17:00, 22:00, 23:00
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

        var command = new CalculateHoldingsBusCommand(Guid.NewGuid())
        {
            AsOfDate = DateOnlyHelper.TodayLocal()
        };
        
        await _bus.Send(command);
    }

    // ReSharper disable once MemberCanBePrivate.Global public required for Hangfire background jobs
    public async Task SendRefreshQuotesCommand()
    {
        _logger.LogInformation("Enqueuing RefreshQuotesCommand...");
        await _bus.Send(new RefreshQuotesBusCommand(Guid.NewGuid()));
    }

    // ReSharper disable once MemberCanBePrivate.Global public required for Hangfire background jobs
    public async Task SendRefreshPricesCommand()
    {
        _logger.LogInformation("Enqueuing RefreshPricesCommand...");

        var command = new RefreshPricesBusCommand(Guid.NewGuid())
        {
            AsOfDate = DateOnlyHelper.TodayLocal()
        };
        
        await _bus.Send(command);
    }
}