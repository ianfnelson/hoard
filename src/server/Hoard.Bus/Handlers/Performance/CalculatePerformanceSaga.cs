using Hoard.Core.Application;
using Hoard.Core.Application.Performance;
using Hoard.Messages.Performance;
using Microsoft.Extensions.Logging;
using Rebus.Bus;
using Rebus.Handlers;
using Rebus.Sagas;

namespace Hoard.Bus.Handlers.Performance;

public class CalculatePerformanceSaga(IMediator mediator, IBus bus, ILogger<CalculatePerformanceSaga> logger)
:
    Saga<CalculatePerformanceSagaData>,
    IAmInitiatedBy<StartCalculatePerformanceSagaCommand>,
    IHandleMessages<PositionPerformanceCalculatedEvent>,
    IHandleMessages<PortfolioPerformanceCalculatedEvent>
{
    protected override void CorrelateMessages(ICorrelationConfig<CalculatePerformanceSagaData> config)
    {
        config.Correlate<StartCalculatePerformanceSagaCommand>(m => m.CorrelationId, d => d.CorrelationId);
        config.Correlate<PositionPerformanceCalculatedEvent>(m => m.CorrelationId, d => d.CorrelationId);
        config.Correlate<PortfolioPerformanceCalculatedEvent>(m => m.CorrelationId, d => d.CorrelationId);
    }
    
    public async Task Handle(StartCalculatePerformanceSagaCommand message)
    {
        var (correlationId, instrumentId, pipelineMode) = message;
        
        Data.CorrelationId = correlationId;

        var instrumentIds = await mediator.QueryAsync<GetInstrumentsForPerformanceQuery, IReadOnlyList<int>>(
            new GetInstrumentsForPerformanceQuery(instrumentId));
        
        logger.LogInformation("Starting position performance calculation for {InstrumentIdsCount} instruments", instrumentIds.Count);
        
        Data.PendingInstruments = instrumentIds.ToHashSet();
        
        await mediator.SendAsync(new DispatchCalculatePositionPerformanceCommand(correlationId, instrumentIds, pipelineMode));
    }

    public async Task Handle(PositionPerformanceCalculatedEvent message)
    {
        Data.PendingInstruments.Remove(message.InstrumentId);
        if (Data.PendingInstruments.Count == 0)
        {
            logger.LogInformation("All position performance calculated");
            
            var portfolioIds =
                await mediator.QueryAsync<GetPortfoliosForPerformanceQuery, IReadOnlyList<int>>(
                    new GetPortfoliosForPerformanceQuery());
        
            logger.LogInformation("Starting portfolio performance calculation for {PortfolioIdsCount} portfolios", portfolioIds.Count);
        
            Data.PendingPortfolios = portfolioIds.ToHashSet();
        
            await mediator.SendAsync(new DispatchCalculatePortfolioPerformanceCommand(message.CorrelationId, portfolioIds, message.PipelineMode));
        }
    }

    public async Task Handle(PortfolioPerformanceCalculatedEvent message)
    {
        Data.PendingPortfolios.Remove(message.PortfolioId);
        if (Data.PendingPortfolios.Count == 0)
        {
            logger.LogInformation("All portfolio performance calculated");
            
            logger.LogInformation("Calculate performance saga {CorrelationId} complete", Data.CorrelationId);
            MarkAsComplete();
        
            await bus.Publish(new PerformanceCalculatedEvent(Data.CorrelationId, message.PipelineMode));
        }
    }
}

public class CalculatePerformanceSagaData : SagaData
{
    public Guid CorrelationId { get; set; }
    public HashSet<int> PendingInstruments { get; set; } = [];
    public HashSet<int> PendingPortfolios { get; set; } = [];
}