using Hoard.Core.Application;
using Hoard.Core.Application.Performance;
using Hoard.Messages.Performances;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;
using Rebus.Sagas;

namespace Hoard.Bus.Handlers.Performance;

public class BackfillPerformancesSaga(IMediator mediator, ILogger<BackfillPerformancesSaga> logger)
:
    Saga<BackfillPerformancesSagaData>,
    IAmInitiatedBy<StartBackfillPerformancesSagaCommand>,
    IHandleMessages<PositionPerformancesCalculatedEvent>,
    IHandleMessages<PortfolioPerformancesCalculatedEvent>
{
    protected override void CorrelateMessages(ICorrelationConfig<BackfillPerformancesSagaData> config)
    {
        config.Correlate<StartBackfillPerformancesSagaCommand>(m => m.CorrelationId, d => d.CorrelationId);
        config.Correlate<PositionPerformancesCalculatedEvent>(m => m.CorrelationId, d => d.CorrelationId);
        config.Correlate<PortfolioPerformancesCalculatedEvent>(m => m.CorrelationId, d => d.CorrelationId);
    }

    public async Task Handle(StartBackfillPerformancesSagaCommand message)
    {
        var (correlationId, instrumentId) = message;
        
        Data.CorrelationId = correlationId;

        var instrumentIds = await mediator.QueryAsync<GetInstrumentsForBackfillQuery, IReadOnlyList<int>>(
            new GetInstrumentsForBackfillQuery(instrumentId));
        
        logger.LogInformation("Starting performance recomputation for {InstrumentIdsCount} instruments", instrumentIds.Count);
        
        Data.PendingInstruments = instrumentIds.ToHashSet();
        
        await mediator.SendAsync(new DispatchBackfillPositionPerformancesCommand(correlationId, instrumentIds));
    }

    public async Task Handle(PositionPerformancesCalculatedEvent message)
    {
        Data.PendingInstruments.Remove(message.InstrumentId);
        if (Data.PendingInstruments.Count == 0)
        {
            logger.LogInformation("All position performances calculated");
        }

        await mediator.SendAsync(new ProcessCalculatePortfolioPerformancesCommand(message.CorrelationId));
    }

    public Task Handle(PortfolioPerformancesCalculatedEvent message)
    {
        logger.LogInformation("Portfolio performances calculated");
        MarkAsComplete();
        
        return Task.CompletedTask;
    }
}

public class BackfillPerformancesSagaData : SagaData
{
    public Guid CorrelationId { get; set; }
    public HashSet<int> PendingInstruments { get; set; } = new();
}