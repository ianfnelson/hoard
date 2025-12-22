using Hoard.Core.Application;
using Hoard.Core.Application.Snapshots;
using Hoard.Messages;
using Hoard.Messages.Snapshots;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;
using Rebus.Sagas;

namespace Hoard.Bus.Handlers.Snapshots;

public class BackfillSnapshotsSaga(
    IMediator mediator,
    ILogger<BackfillSnapshotsSaga> logger)
    : 
        Saga<BackfillSnapshotsSagaData>,
        IAmInitiatedBy<StartBackfillSnapshotsSagaCommand>,
        IHandleMessages<SnapshotsCalculatedEvent>
{
    protected override void CorrelateMessages(ICorrelationConfig<BackfillSnapshotsSagaData> config)
    {
        config.Correlate<StartBackfillSnapshotsSagaCommand>(m => m.SnapshotsRunId, d => d.SnapshotsRunId);
        config.Correlate<SnapshotsCalculatedEvent>(m => m.SnapshotsRunId, d => d.SnapshotsRunId);
    }

    public async Task Handle(StartBackfillSnapshotsSagaCommand message)
    {
        var (snapshotsRunId, pipelineMode, portfolioId, startYear, endYear) = message;

        Data.SnapshotsRunId = snapshotsRunId;
        Data.PipelineMode = pipelineMode;
        Data.PortfolioId = portfolioId;

        var years = await mediator.QueryAsync<GetYearsForBackfillQuery, IReadOnlyList<int>>(
            new GetYearsForBackfillQuery(startYear, endYear));

        Data.StartYear = years.Min();
        Data.EndYear = years.Max();
        
        logger.LogInformation("Starting snapshots recomputation {Start} → {End}", Data.StartYear, Data.EndYear);

        Data.PendingYears = years.ToHashSet();
        
        await mediator.SendAsync(new DispatchBackfillSnapshotsCommand(snapshotsRunId, pipelineMode, portfolioId, years));
    }

    public Task Handle(SnapshotsCalculatedEvent message)
    {
        Data.PendingYears.Remove(message.Year);
        if (Data.PendingYears.Count == 0)
        {
            logger.LogInformation("All snapshots recomputed. Done!");
            MarkAsComplete();
        }
        
        return Task.CompletedTask;
    }
}

public class BackfillSnapshotsSagaData : SagaData
{
    public Guid SnapshotsRunId { get; set; }
    public PipelineMode PipelineMode { get; set; }
    public int? PortfolioId { get; set; }
    public int StartYear { get; set; }
    public int EndYear { get; set; }

    public HashSet<int> PendingYears { get; set; } = new();
}