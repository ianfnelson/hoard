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

    private static string BuildCorrelationKey(Guid correlationId, DateOnly asOfDate)
        => $"{correlationId:N}:{asOfDate:yyyyMMdd}";
    
    protected override void CorrelateMessages(ICorrelationConfig<PortfolioValuationsRecalculationSagaData> config)
    {
        config.Correlate<PortfolioValuationsInvalidatedEvent>(
            m => BuildCorrelationKey(m.CorrelationId, m.AsOfDate), 
            d => d.CorrelationKey
            );
        
        config.Correlate<RecalculatePortfolioValuationsTimeout>(
            m => BuildCorrelationKey(m.CorrelationId, m.AsOfDate), 
            d => d.CorrelationKey
        );
    }

    public async Task Handle(PortfolioValuationsInvalidatedEvent message)
    {
        if (Data.IsScheduled)
        {
            return;
        }
        
        Data.IsScheduled = true;
        Data.CorrelationId = message.CorrelationId;
        Data.AsOfDate = message.AsOfDate;
        Data.CorrelationKey = BuildCorrelationKey(message.CorrelationId, message.AsOfDate);

        await bus.DeferLocal(
            DebounceDelay,
            new RecalculatePortfolioValuationsTimeout(message.CorrelationId, message.PipelineMode, message.AsOfDate));
    }

    public async Task Handle(RecalculatePortfolioValuationsTimeout message)
    {
        var portfolioIds = 
            await mediator.QueryAsync<GetPortfoliosForValuationQuery, IReadOnlyList<int>> (
                new GetPortfoliosForValuationQuery());
            
        await mediator.SendAsync(new DispatchCalculatePortfolioValuationCommand(message.CorrelationId, message.PipelineMode, portfolioIds, message.AsOfDate));
    }
}

public class PortfolioValuationsRecalculationSagaData : SagaData
{
    public Guid CorrelationId { get; set; }
    public DateOnly AsOfDate { get; set; }
    public string CorrelationKey { get; set; } = null!;
    public bool IsScheduled { get; set; }
}

public record RecalculatePortfolioValuationsTimeout(Guid CorrelationId, PipelineMode PipelineMode, DateOnly AsOfDate);