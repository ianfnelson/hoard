using Hoard.Core.Application;
using Hoard.Core.Application.Performance;
using Hoard.Messages;
using Hoard.Messages.Performance;
using Rebus.Bus;
using Rebus.Handlers;
using Rebus.Sagas;

namespace Hoard.Bus.Performance;

public class PortfolioPerformanceRecalculationSaga(IBus bus, IMediator mediator) : 
    Saga<PprSagaData>,
    IAmInitiatedBy<PortfolioPerformancesInvalidatedEvent>,
    IHandleMessages<RecalculatePortfolioPerformancesTimeout>
{
    private static readonly TimeSpan DebounceDelay = TimeSpan.FromSeconds(5);
    
    protected override void CorrelateMessages(ICorrelationConfig<PprSagaData> config)
    {
        config.Correlate<PortfolioPerformancesInvalidatedEvent>(_ => DebounceScopes.PortfolioPerformances, d => d.Scope);
        config.Correlate<RecalculatePortfolioPerformancesTimeout>(_ => DebounceScopes.PortfolioPerformances, d => d.Scope);
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
            new RecalculatePortfolioPerformancesTimeout(message.PipelineMode));
    }

    public async Task Handle(RecalculatePortfolioPerformancesTimeout message)
    {
        var portfolioIds =
            await mediator.QueryAsync<GetPortfoliosForPerformanceQuery, IReadOnlyList<int>>(
                new GetPortfoliosForPerformanceQuery());
        
        await mediator.SendAsync(new DispatchCalculatePortfolioPerformanceCommand(Guid.NewGuid(), portfolioIds, message.PipelineMode));
        
        Data.IsScheduled = false;
        
        MarkAsComplete();
    }
}

public class PprSagaData : SagaData
{
    public string Scope { get; set; } = DebounceScopes.PortfolioPerformances;
    public bool IsScheduled { get; set; }
}

public record RecalculatePortfolioPerformancesTimeout(PipelineMode PipelineMode);