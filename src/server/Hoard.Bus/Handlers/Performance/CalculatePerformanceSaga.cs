using Hoard.Core.Application;
using Hoard.Core.Application.Performance;
using Hoard.Messages.Performance;
using Hoard.Messages.Positions;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;
using Rebus.Sagas;

namespace Hoard.Bus.Handlers.Performance;

public class CalculatePerformanceSaga(IMediator mediator, ILogger<CalculatePerformanceSaga> logger)
:
    Saga<CalculatePerformanceSagaData>,
    IAmInitiatedBy<StartCalculatePerformanceSagaCommand>,
    IAmInitiatedBy<PositionsCalculatedEvent>,
    IHandleMessages<PositionPerformanceCalculatedEvent>,
    IHandleMessages<PortfolioPerformanceCalculatedEvent>
{
    protected override void CorrelateMessages(ICorrelationConfig<CalculatePerformanceSagaData> config)
    {
        config.Correlate<PositionsCalculatedEvent>(m => m.CorrelationId, d => d.CorrelationId);
        config.Correlate<StartCalculatePerformanceSagaCommand>(m => m.CorrelationId, d => d.CorrelationId);
        config.Correlate<PositionPerformanceCalculatedEvent>(m => m.CorrelationId, d => d.CorrelationId);
        config.Correlate<PortfolioPerformanceCalculatedEvent>(m => m.CorrelationId, d => d.CorrelationId);
    }

    public async Task Handle(PositionsCalculatedEvent message)
    {
        await Start(message.CorrelationId, null);
    }
    
    public async Task Handle(StartCalculatePerformanceSagaCommand message)
    {
        await Start(message.CorrelationId, message.InstrumentId);
    }

    private async Task Start(Guid correlationId, int? instrumentId)
    {
        Data.CorrelationId = correlationId;

        var instrumentIds = await mediator.QueryAsync<GetInstrumentsForPerformanceQuery, IReadOnlyList<int>>(
            new GetInstrumentsForPerformanceQuery(instrumentId));
        
        logger.LogInformation("Starting performance recomputation for {InstrumentIdsCount} instruments", instrumentIds.Count);
        
        Data.PendingInstruments = instrumentIds.ToHashSet();
        
        await mediator.SendAsync(new DispatchCalculatePositionPerformanceCommand(correlationId, instrumentIds));
    }

    public async Task Handle(PositionPerformanceCalculatedEvent message)
    {
        Data.PendingInstruments.Remove(message.InstrumentId);
        if (Data.PendingInstruments.Count == 0)
        {
            logger.LogInformation("All position performance calculated");
        }

        await mediator.SendAsync(new ProcessCalculatePortfolioPerformanceCommand(message.CorrelationId));
    }

    public Task Handle(PortfolioPerformanceCalculatedEvent message)
    {
        logger.LogInformation("Portfolio performance calculated");
        MarkAsComplete();
        
        return Task.CompletedTask;
    }
}

public class CalculatePerformanceSagaData : SagaData
{
    public Guid CorrelationId { get; set; }
    public HashSet<int> PendingInstruments { get; set; } = new();
}