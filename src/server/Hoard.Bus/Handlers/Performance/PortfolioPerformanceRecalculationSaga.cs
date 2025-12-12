using Hoard.Core.Application;
using Hoard.Core.Application.Performance;
using Hoard.Messages;
using Hoard.Messages.Performance;
using Rebus.Bus;
using Rebus.Handlers;
using Rebus.Sagas;

namespace Hoard.Bus.Handlers.Performance;

public class PortfolioPerformanceRecalculationSaga(IBus bus, IMediator mediator) : 
    Saga<PortfolioPerformanceRecalculationSagaData>,
    IAmInitiatedBy<PortfolioPerformancesInvalidatedEvent>,
    IHandleMessages<RecalculatePortfolioPerformancesTimeout>
{
    private static readonly TimeSpan DebounceDelay = TimeSpan.FromSeconds(5);
    
    protected override void CorrelateMessages(ICorrelationConfig<PortfolioPerformanceRecalculationSagaData> config)
    {
        config.Correlate<PortfolioPerformancesInvalidatedEvent>(m => m.CorrelationId, d => d.CorrelationId);
        config.Correlate<RecalculatePortfolioPerformancesTimeout>(m => m.CorrelationId, d => d.CorrelationId);
    }

    public async Task Handle(PortfolioPerformancesInvalidatedEvent message)
    {
        if (Data.IsScheduled)
        {
            return;
        }
        
        Data.IsScheduled = true;

        await bus.DeferLocal(
            DebounceDelay,
            new RecalculatePortfolioPerformancesTimeout(message.CorrelationId, message.PipelineMode));
    }

    public async Task Handle(RecalculatePortfolioPerformancesTimeout message)
    {
        var portfolioIds =
            await mediator.QueryAsync<GetPortfoliosForPerformanceQuery, IReadOnlyList<int>>(
                new GetPortfoliosForPerformanceQuery());
        
        await mediator.SendAsync(new DispatchCalculatePortfolioPerformanceCommand(message.CorrelationId, portfolioIds, message.PipelineMode));
    }
}

public class PortfolioPerformanceRecalculationSagaData : SagaData
{
    public Guid CorrelationId { get; set; }
    public bool IsScheduled { get; set; }
}

public record RecalculatePortfolioPerformancesTimeout(Guid CorrelationId, PipelineMode PipelineMode);