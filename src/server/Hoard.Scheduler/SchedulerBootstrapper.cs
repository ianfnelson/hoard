using System.Diagnostics;
using Hangfire;
using Hoard.Core;
using Hoard.Core.Application;
using Hoard.Core.Application.Chrono;
using Hoard.Core.Application.News;
using Hoard.Core.Application.Performance;
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
    private static readonly TimeZoneInfo UkTimeZone = DateOnlyHelper.UkTimeZone;

    private readonly TimeProvider _timeProvider;

    public SchedulerBootstrapper(
        IBackgroundJobClient jobs,
        IRecurringJobManager recurring,
        IMediator mediator,
        ILogger<SchedulerBootstrapper> logger,
        TimeProvider timeProvider)
    {
        _jobs = jobs;
        _recurring = recurring;
        _mediator = mediator;
        _logger = logger;
        _timeProvider = timeProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Registering Hoard Scheduler jobs...");

        RegisterStartOfDay();
        RegisterCloseOfDay();
        RegisterRefreshQuotes();
        RegisterRefreshNews();

        return Task.CompletedTask;
    }
    
    private void RegisterRefreshQuotes()
    {
        _recurring.AddOrUpdate(
            "refresh-quotes",
            () => TriggerRefreshQuotesCommand(),
            "*/1 8-17 * * 1-5", // every minute during UK trading hours
            new RecurringJobOptions { TimeZone = UkTimeZone }
        );
    }

    private void RegisterRefreshNews()
    {
        _recurring.AddOrUpdate(
            "refresh-news",
            () => TriggerRefreshNewsCommand(),
            "10 7-17 * * 1-5", // 10 minutes past every hour, 7am-5pm, weekdays
            new RecurringJobOptions { TimeZone = UkTimeZone }
        );
    }

    private void RegisterStartOfDay()
    {
        _recurring.AddOrUpdate(
            "start-of-day",
            () => TriggerStartOfDayCommand(),
            "0 6 * * *", // daily at 06:00
            new RecurringJobOptions { TimeZone = UkTimeZone }
        );
    }

    private void RegisterCloseOfDay()
    {
        _recurring.AddOrUpdate(
            "close-of-day",
            () => TriggerCloseOfDayCommand(),
            "0 17 * * *", // daily at 17:00
            new RecurringJobOptions { TimeZone = UkTimeZone }
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
    public async Task TriggerRefreshNewsCommand()
    {
        using var activity = ActivitySource.StartActivity();

        _logger.LogInformation("Triggering Refresh News");

        var command = new TriggerRefreshNewsCommand(null);

        await _mediator.SendAsync(command);
    }

    // ReSharper disable once MemberCanBePrivate.Global public required for Hangfire background jobs
    public async Task TriggerCloseOfDayCommand()
    {
        using var activity = ActivitySource.StartActivity();
        
        _logger.LogInformation("Triggering Close Of Day");

        var today = DateOnlyHelper.TodayLocal(_timeProvider);
        
        var command = new TriggerCloseOfDayRunCommand(today);
        
        await _mediator.SendAsync(command);
    }
    
    // ReSharper disable once MemberCanBePrivate.Global public required for Hangfire background jobs
    public async Task TriggerStartOfDayCommand()
    {
        using var activity = ActivitySource.StartActivity();
        
        _logger.LogInformation("Triggering Start Of Day");
        
        var command = new TriggerCalculatePerformanceCommand(null);
        
        await _mediator.SendAsync(command);
    }
}