using Hoard.Core.Application;
using Hoard.Core.Application.Performance;
using Hoard.Core.Application.Shared;
using Hoard.Messages.Performance;
using Microsoft.Extensions.Logging;
using Rebus.Bus;
using Rebus.Handlers;
using Rebus.Sagas;

namespace Hoard.Bus.Performance;

public class CalculatePerformanceSaga(IMediator mediator, IBus bus, ILogger<CalculatePerformanceSaga> logger)
:
    Saga<CpSagaData>,
    IAmInitiatedBy<StartCalculatePerformanceSagaCommand>,
    IHandleMessages<PositionPerformanceCalculatedEvent>,
    IHandleMessages<PortfolioPerformanceCalculatedEvent>
{
    protected override void CorrelateMessages(ICorrelationConfig<CpSagaData> config)
    {
        config.Correlate<StartCalculatePerformanceSagaCommand>(m => m.PerformanceRunId, d => d.PerformanceRunId);
        config.Correlate<PositionPerformanceCalculatedEvent>(m => m.PerformanceRunId, d => d.PerformanceRunId);
        config.Correlate<PortfolioPerformanceCalculatedEvent>(m => m.PerformanceRunId, d => d.PerformanceRunId);
    }
    
    public async Task Handle(StartCalculatePerformanceSagaCommand message)
    {
        var (performanceRunId, instrumentId, pipelineMode) = message;
        
        Data.PerformanceRunId = performanceRunId;

        var instrumentIds = await mediator.QueryAsync<GetInstrumentsForPerformanceQuery, IReadOnlyList<int>>(
            new GetInstrumentsForPerformanceQuery(instrumentId));
        
        logger.LogInformation("Starting position performance calculation for {InstrumentIdsCount} instruments", instrumentIds.Count);
        
        Data.PendingInstruments = instrumentIds.ToHashSet();
        
        await mediator.SendAsync(new DispatchCalculatePositionPerformanceCommand(performanceRunId, instrumentIds, pipelineMode));
    }

    public async Task Handle(PositionPerformanceCalculatedEvent message)
    {
        Data.PendingInstruments.Remove(message.InstrumentId);
        if (Data.PendingInstruments.Count == 0)
        {
            logger.LogInformation("All position performance calculated");
            
            var portfolioIds =
                await mediator.QueryAsync<GetPortfolioIdsQuery, IReadOnlyList<int>>(
                    new GetPortfolioIdsQuery());
        
            logger.LogInformation("Starting portfolio performance calculation for {PortfolioIdsCount} portfolios", portfolioIds.Count);
        
            Data.PendingPortfolios = portfolioIds.ToHashSet();
        
            await mediator.SendAsync(new DispatchCalculatePortfolioPerformanceCommand(message.PerformanceRunId, portfolioIds, message.PipelineMode));
        }
    }

    public async Task Handle(PortfolioPerformanceCalculatedEvent message)
    {
        Data.PendingPortfolios.Remove(message.PortfolioId);
        if (Data.PendingPortfolios.Count == 0)
        {
            logger.LogInformation("All portfolio performance calculated");
            
            logger.LogInformation("Calculate performance saga {PerformanceRunId} complete", Data.PerformanceRunId);
            MarkAsComplete();
        
            await bus.Publish(new PerformanceCalculatedEvent(Data.PerformanceRunId, message.PipelineMode));
        }
    }
}

public class CpSagaData : SagaData
{
    public Guid PerformanceRunId { get; set; }
    public HashSet<int> PendingInstruments { get; set; } = [];
    public HashSet<int> PendingPortfolios { get; set; } = [];
}