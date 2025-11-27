using Hangfire;
using Hoard.Core;
using Hoard.Core.Application;
using Hoard.Core.Application.Holdings;
using Hoard.Core.Application.Prices;
using Hoard.Core.Application.Quotes;
using Hoard.Core.Application.Valuations;
using Hoard.Messages;
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
        RegisterCalculateValuations();
        
        return Task.CompletedTask;
    }

    private void RegisterCalculateHoldings()
    {
        _recurring.AddOrUpdate(
            "calculate-holdings",
            () => TriggerCalculateHoldingsCommand(),
            "0 2 * * *" // nightly at 2am
        );
    }
    
    private void RegisterRefreshQuotes()
    {
        _recurring.AddOrUpdate(
            "refresh-quotes",
            () => TriggerRefreshQuotesCommand(),
            "*/2 8-17 * * 1-5" // every two minutes during UK trading hours
        );
    }

    private void RegisterRefreshPrices()
    {
        _recurring.AddOrUpdate(
            "refresh-prices",
            () => TriggerRefreshPricesCommand(),
            "45 17,21,22 * * 1-5" // weekdays at 17:45, 21:45, 22:45
        );
    }
    
    private void RegisterCalculateValuations()
    {
        _recurring.AddOrUpdate(
            "calculate-valuations",
            () => TriggerCalculateValuationsCommand(),
            "15 23 * * 1-5" // weekdays 23:15
        );
    }
    
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Scheduler stopping...");
        return Task.CompletedTask;
    }

    // ReSharper disable once MemberCanBePrivate.Global public required for Hangfire background jobs
    public async Task TriggerCalculateHoldingsCommand()
    {
        _logger.LogInformation("Triggering Calculate Holdings");

        var command = new TriggerCalculateHoldingsCommand( 
            Guid.NewGuid(), 
            PipelineMode.NightPreMidnight,
            DateOnlyHelper.TodayLocal());
        
        await _mediator.SendAsync(command);
    }

    // ReSharper disable once MemberCanBePrivate.Global public required for Hangfire background jobs
    public async Task TriggerRefreshQuotesCommand()
    {
        _logger.LogInformation("Triggering Refresh Quotes");

        var command = new TriggerRefreshQuotesCommand(Guid.NewGuid(), PipelineMode.NightPreMidnight);

        await _mediator.SendAsync(command);
    }

    // ReSharper disable once MemberCanBePrivate.Global public required for Hangfire background jobs
    public async Task TriggerRefreshPricesCommand()
    {
        _logger.LogInformation("Triggering Refresh Prices");

        var today = DateOnlyHelper.TodayLocal();
        
        var command = new TriggerRefreshPricesCommand(
            Guid.NewGuid(), 
            PipelineMode.NightPreMidnight, 
            null, 
            today, today);
        
        await _mediator.SendAsync(command);
    }
    
    // ReSharper disable once MemberCanBePrivate.Global public required for Hangfire background jobs
    public async Task TriggerCalculateValuationsCommand()
    {
        _logger.LogInformation("Triggering Calculate Valuations");

        var command = new TriggerCalculateValuationsCommand(Guid.NewGuid(), DateOnlyHelper.TodayLocal());
        
        await _mediator.SendAsync(command);
    }
}