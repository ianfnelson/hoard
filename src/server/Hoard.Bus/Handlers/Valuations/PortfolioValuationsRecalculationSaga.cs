using Hoard.Core.Application;
using Hoard.Core.Application.Valuations;
using Hoard.Messages;
using Hoard.Messages.Valuations;
using Rebus.Bus;
using Rebus.Handlers;
using Rebus.Sagas;

namespace Hoard.Bus.Handlers.Valuations;

public class PortfolioValuationsRecalculationSaga(IBus bus, IMediator mediator) :
    Saga<PortfolioValuationsRecalculationSagaData>,
    IAmInitiatedBy<PortfolioValuationsInvalidatedEvent>,
    IHandleMessages<RecalculatePortfolioValuationsTimeout>
{
    private static readonly TimeSpan DebounceDelay = TimeSpan.FromSeconds(3);
    
    protected override void CorrelateMessages(ICorrelationConfig<PortfolioValuationsRecalculationSagaData> config)
    {
        config.Correlate<PortfolioValuationsInvalidatedEvent>(
            m => m.AsOfDate.ToString("yyyy-MM-dd"), 
            d => d.AsOfDate.ToString("yyyy-MM-dd")
            );
        
        config.Correlate<RecalculatePortfolioValuationsTimeout>(
            m => m.AsOfDate.ToString("yyyy-MM-dd"), 
            d => d.AsOfDate.ToString("yyyy-MM-dd")
        );
    }

    public async Task Handle(PortfolioValuationsInvalidatedEvent message)
    {
        if (Data.IsScheduled)
        {
            return;
        }
        
        Data.IsScheduled = true;
        Data.AsOfDate = message.AsOfDate;

        await bus.DeferLocal(
            DebounceDelay,
            new RecalculatePortfolioValuationsTimeout(message.PipelineMode, message.AsOfDate));
    }

    public async Task Handle(RecalculatePortfolioValuationsTimeout message)
    {
        var portfolioIds = 
            await mediator.QueryAsync<GetPortfoliosForValuationQuery, IReadOnlyList<int>> (
                new GetPortfoliosForValuationQuery());
            
        await mediator.SendAsync(new DispatchCalculatePortfolioValuationCommand(Guid.NewGuid(), message.PipelineMode, portfolioIds, message.AsOfDate));
        
        Data.IsScheduled = false;
        MarkAsComplete();
    }
}

public class PortfolioValuationsRecalculationSagaData : SagaData
{
    public string Scope { get; set; } = DebounceScopes.PortfolioValuations;
    public DateOnly AsOfDate { get; set; }
    public bool IsScheduled { get; set; }
}

public record RecalculatePortfolioValuationsTimeout(PipelineMode PipelineMode, DateOnly AsOfDate);