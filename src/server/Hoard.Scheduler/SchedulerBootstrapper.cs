using Hangfire;
using Hoard.Core;
using Hoard.Core.Application;
using Hoard.Core.Application.Chrono;
using Hoard.Core.Application.Quotes;
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

        RegisterNightlyPreMidnight();
        RegisterNightlyPostMidnight();
        RegisterRefreshQuotes();
        
        return Task.CompletedTask;
    }
    
    private void RegisterRefreshQuotes()
    {
        _recurring.AddOrUpdate(
            "refresh-quotes",
            () => TriggerRefreshQuotesCommand(),
            "*/2 8-17 * * 1-5" // every two minutes during UK trading hours
        );
    }

    private void RegisterNightlyPreMidnight()
    {
        _recurring.AddOrUpdate(
            "refresh-prices",
            () => TriggerNightlyPreMidnightCommand(),
            "45 17,22 * * 1-5" // weekdays at 17:45 and 22:45
        );
    }
    
    private void RegisterNightlyPostMidnight()
    {
        _recurring.AddOrUpdate(
            "refresh-prices",
            () => TriggerNightlyPostMidnightCommand(),
            "1 0 * * *" // daily at 00:01
        );
    }
    
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Scheduler stopping...");
        return Task.CompletedTask;
    }


    // ReSharper disable once MemberCanBePrivate.Global public required for Hangfire background jobs
    public async Task TriggerRefreshQuotesCommand()
    {
        _logger.LogInformation("Triggering Refresh Quotes");

        var command = new TriggerRefreshQuotesCommand(Guid.NewGuid(), PipelineMode.NightlyPreMidnight);

        await _mediator.SendAsync(command);
    }

    // ReSharper disable once MemberCanBePrivate.Global public required for Hangfire background jobs
    public async Task TriggerNightlyPreMidnightCommand()
    {
        _logger.LogInformation("Triggering Nightly Pre Midnight");

        var today = DateOnlyHelper.TodayLocal();
        
        var command = new TriggerNightlyPreMidnightRunCommand(Guid.NewGuid(), today);
        
        await _mediator.SendAsync(command);
    }
    
    // ReSharper disable once MemberCanBePrivate.Global public required for Hangfire background jobs
    public async Task TriggerNightlyPostMidnightCommand()
    {
        _logger.LogInformation("Triggering Nightly Post Midnight");

        var today = DateOnlyHelper.TodayLocal();
        
        var command = new TriggerNightlyPostMidnightRunCommand(Guid.NewGuid(), today);
        
        await _mediator.SendAsync(command);
    }
}