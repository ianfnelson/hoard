using Hangfire;
using Hoard.Core.Messages;
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

        RegisterRecalculateHoldings();
        RegisterRefreshQuotes();
        RegisterFetchDailyPrices();
        RegisterRecalculateValuations();
        
        return Task.CompletedTask;
    }

    private void RegisterRecalculateHoldings()
    {
        _recurring.AddOrUpdate(
            "recalculate-holdings",
            () => SendRecalculateHoldingsCommand(),
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

    private void RegisterFetchDailyPrices()
    {
        _recurring.AddOrUpdate(
            "fetch-daily-prices",
            () => SendFetchDailyPricesCommand(),
            "40 21 * * *" // daily at 21:40
        );
    }
    
    private void RegisterRecalculateValuations()
    {
        _recurring.AddOrUpdate(
            "recalculate-valuations",
            () => SendRecalculateValuationsCommand(),
            "0 23 * * *" // daily at 23:00
        );
    }
    
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Scheduler stopping...");
        return Task.CompletedTask;
    }

    // ReSharper disable once MemberCanBePrivate.Global public required for Hangfire background jobs
    public async Task SendRecalculateHoldingsCommand()
    {
        _logger.LogInformation("Enqueuing RecalculateHoldingsCommand...");
        await _bus.Send(new RecalculateHoldingsCommand(DateOnly.FromDateTime(DateTime.Now)));
    }

    // ReSharper disable once MemberCanBePrivate.Global public required for Hangfire background jobs
    public async Task SendRefreshQuotesCommand()
    {
        _logger.LogInformation("Enqueuing RefreshQuotesCommand...");
        await _bus.Send(new RefreshQuotesCommand());
    }

    // ReSharper disable once MemberCanBePrivate.Global public required for Hangfire background jobs
    public async Task SendFetchDailyPricesCommand()
    {
        _logger.LogInformation("Enqueuing FetchDailyPricesCommand...");
        await _bus.Send(new FetchDailyPricesCommand(DateOnly.FromDateTime(DateTime.Now)));
    }
    
    // ReSharper disable once MemberCanBePrivate.Global public required for Hangfire background jobs
    public async Task SendRecalculateValuationsCommand()
    {
        _logger.LogInformation("Enqueuing RecalculateValuationsCommand...");
        await _bus.Send(new RecalculateValuationsCommand(DateOnly.FromDateTime(DateTime.Now)));
    }
}