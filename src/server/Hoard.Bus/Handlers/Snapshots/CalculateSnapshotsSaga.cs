using Hoard.Core.Application;
using Hoard.Core.Application.Snapshots;
using Hoard.Messages.Snapshots;
using Microsoft.Extensions.Logging;
using Rebus.Bus;
using Rebus.Handlers;
using Rebus.Sagas;

namespace Hoard.Bus.Handlers.Snapshots;

public class CalculateSnapshotsSaga(ILogger<CalculateSnapshotsSaga> logger, IMediator mediator, IBus bus)
    :
        Saga<CalculateSnapshotsSagaData>,
        IAmInitiatedBy<StartCalculateSnapshotsSagaCommand>,
        IHandleMessages<SnapshotCalculatedEvent>
{
    protected override void CorrelateMessages(ICorrelationConfig<CalculateSnapshotsSagaData> config)
    {
        config.Correlate<StartCalculateSnapshotsSagaCommand>(
            m => $"{m.CorrelationId:N}:{m.Year}",
            d => d.CorrelationKey);

        config.Correlate<SnapshotCalculatedEvent>(
            m => $"{m.CorrelationId:N}:{m.Year}",
            d => d.CorrelationKey);
    }

    public async Task Handle(StartCalculateSnapshotsSagaCommand message)
    {
        var (correlationId, pipelineMode, portfolioId, nullableYear) = message;

        Data.CorrelationId = correlationId;

        var year = nullableYear ?? DateTime.Today.Year;
        Data.Year = year;

        var portfolioIds = await mediator.QueryAsync<GetPortfoliosForSnapshotQuery, IReadOnlyList<int>>(
            new GetPortfoliosForSnapshotQuery(portfolioId));

        if (portfolioIds.Count == 0)
        {
            MarkAsComplete();
            return;
        }
        
        logger.LogInformation("Started calculate snapshots saga {CorrelationKey} for {Count} portfolios",
            Data.CorrelationKey, portfolioIds.Count);

        Data.PendingPortfolios = portfolioIds.ToHashSet();

        await mediator.SendAsync(new DispatchCalculateSnapshotCommand(correlationId, pipelineMode, portfolioIds, year));
    }

    public async Task Handle(SnapshotCalculatedEvent message)
    {
        var (correlationId, pipelineMode, year, portfolioId) = message;

        Data.PendingPortfolios.Remove(portfolioId);

        if (Data.PendingPortfolios.Count == 0)
        {
            logger.LogInformation("Calculate snapshots saga {CorrelationKey} complete", Data.CorrelationKey);
            MarkAsComplete();
            await bus.Publish(new SnapshotsCalculatedEvent(correlationId, pipelineMode, year));
        }
    }
}

public class CalculateSnapshotsSagaData : SagaData
{
    public Guid CorrelationId { get; set; }
    public int Year { get; set; }
    
    public string CorrelationKey => $"{CorrelationId:N}:{Year}";

    public HashSet<int> PendingPortfolios { get; set; } = new();
}