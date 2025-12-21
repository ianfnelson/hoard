using System.Diagnostics;
using Hangfire;
using Hoard.Core;
using Hoard.Core.Application;
using Hoard.Core.Application.Chrono;
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
    
    private static readonly ActivitySource ActivitySource = new("hoard.scheduler");

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

        RegisterCloseOfDay();
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

    private void RegisterCloseOfDay()
    {
        _recurring.AddOrUpdate(
            "close-of-day",
            () => TriggerCloseOfDayCommand(),
            "0 18 * * *" // daily at 18:00
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
        using var activity = ActivitySource .StartActivity();
        
        _logger.LogInformation("Triggering Refresh Quotes");

        var command = new TriggerRefreshQuotesCommand();

        await _mediator.SendAsync(command);
    }

    // ReSharper disable once MemberCanBePrivate.Global public required for Hangfire background jobs
    public async Task TriggerCloseOfDayCommand()
    {
        using var activity = ActivitySource.StartActivity();
        
        _logger.LogInformation("Triggering Close Of Day");

        var today = DateOnlyHelper.TodayLocal();
        
        var command = new TriggerCloseOfDayRunCommand(Guid.NewGuid(), today);
        
        await _mediator.SendAsync(command);
    }
}