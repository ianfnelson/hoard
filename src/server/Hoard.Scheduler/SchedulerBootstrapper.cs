using Hangfire;
using Hoard.Core;
using Hoard.Core.Application;
using Hoard.Core.Application.Holdings;
using Hoard.Core.Application.Prices;
using Hoard.Core.Application.Quotes;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Hoard.Scheduler;

public class SchedulerBootstrapper : IHostedService
{
    private readonly IBackgroundJobClient _jobs;
    private readonly IRecurringJobManager _recurring;
    private readonly IMediator _mediator;
    private readonly ILogger<SchedulerBootstrapper> _logger;

    public SchedulerBootstrapper(
        IBackgroundJobClient jobs,
        IRecurringJobManager recurring,
        IMediator mediator,
        ILogger<SchedulerBootstrapper> logger)
    {
        _jobs = jobs;
        _recurring = recurring;
        _mediator = mediator;
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
            () => TriggerCalculateHoldings(),
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
            "0 17,22,23 * * 1-5" // weekdays at 17:00, 22:00, 23:00
        );
    }
    
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Scheduler stopping...");
        return Task.CompletedTask;
    }

    // ReSharper disable once MemberCanBePrivate.Global public required for Hangfire background jobs
    public async Task TriggerCalculateHoldings()
    {
        _logger.LogInformation("Triggering Calculate Holdings");

        var command = new TriggerCalculateHoldingsCommand( Guid.NewGuid(), DateOnlyHelper.TodayLocal());
        
        await _mediator.SendAsync(command);
    }

    // ReSharper disable once MemberCanBePrivate.Global public required for Hangfire background jobs
    public async Task SendRefreshQuotesCommand()
    {
        _logger.LogInformation("Triggering Refresh Quotes");

        var command = new TriggerRefreshQuotesCommand(Guid.NewGuid());

        await _mediator.SendAsync(command);
    }

    // ReSharper disable once MemberCanBePrivate.Global public required for Hangfire background jobs
    public async Task SendRefreshPricesCommand()
    {
        _logger.LogInformation("Triggering Refresh Prices");

        var command = new TriggerRefreshPricesCommand(Guid.NewGuid(), DateOnlyHelper.TodayLocal());
        
        await _mediator.SendAsync(command);
    }
}