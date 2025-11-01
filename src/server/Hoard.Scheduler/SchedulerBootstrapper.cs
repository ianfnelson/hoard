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

        _recurring.AddOrUpdate(
            "refresh-quotes-batch",
            () => SendRefreshQuotesBatchCommand(),
            "*/15 8-17 * * 1-5" // every 15 minutes during UK trading hours
        );

        _recurring.AddOrUpdate(
            "recalculate-holdings",
            () => SendRecalculateHoldingsCommand(),
            "0 2 * * *" // nightly at 2am
        );

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Scheduler stopping...");
        return Task.CompletedTask;
    }

    public async Task SendRefreshQuotesBatchCommand()
    {
        _logger.LogInformation("Enqueuing RefreshQuotesBatchCommand...");
        await _bus.Send(new RefreshQuotesBatchCommand());
    }

    public async Task SendRecalculateHoldingsCommand()
    {
        _logger.LogInformation("Enqueuing RecalculateHoldingsCommand...");
        await _bus.Send(new RecalculateHoldingsCommand());
    }
}