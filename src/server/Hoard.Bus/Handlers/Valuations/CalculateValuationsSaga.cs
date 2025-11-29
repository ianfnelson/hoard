using Hoard.Core.Application;
using Hoard.Core.Application.Valuations;
using Hoard.Core.Extensions;
using Hoard.Messages.Valuations;
using Microsoft.Extensions.Logging;
using Rebus.Bus;
using Rebus.Handlers;
using Rebus.Sagas;

namespace Hoard.Bus.Handlers.Valuations;

public class CalculateValuationsSaga(ILogger<CalculateValuationsSaga> logger, IMediator mediator, IBus bus)
    :
        Saga<CalculateValuationsSagaData>,
        IAmInitiatedBy<StartCalculateValuationsSagaCommand>,
        IHandleMessages<ValuationsCalculatedForHoldingEvent>
{
    protected override void CorrelateMessages(ICorrelationConfig<CalculateValuationsSagaData> cfg)
    {
        cfg.Correlate<StartCalculateValuationsSagaCommand>(
            m => $"{m.CorrelationId:N}:{m.AsOfDate}",
            d => d.CorrelationKey);

        cfg.Correlate<ValuationsCalculatedForHoldingEvent>(
            m => $"{m.CorrelationId:N}:{m.AsOfDate}",
            d => d.CorrelationKey);
    }

    public async Task Handle(StartCalculateValuationsSagaCommand message)
    {
        var (correlationId, pipelineMode, instrumentId, nullableAsOfDate) = message;
        
        Data.CorrelationId = correlationId;

        var asOfDate = nullableAsOfDate.OrToday();
        Data.AsOfDate = asOfDate;

        var holdingIds = await mediator.QueryAsync<GetInstrumentsForValuationQuery, IReadOnlyList<int>>(
            new GetInstrumentsForValuationQuery(asOfDate, instrumentId));
        
        if (holdingIds.Count == 0)
        {
            MarkAsComplete();
            return;
        }

        logger.LogInformation("Started calculate valuations saga {CorrelationKey} for {Count} holdings",
            Data.CorrelationKey, holdingIds.Count);

        Data.PendingHoldings = holdingIds.ToHashSet();
        
        await mediator.SendAsync(new DispatchValuationsCommand(correlationId, pipelineMode, holdingIds, asOfDate));
    }

    public async Task Handle(ValuationsCalculatedForHoldingEvent message)
    {
        var (correlationId, pipelineMode, holdingId, asOfDate) = message;
        
        Data.PendingHoldings.Remove(holdingId);
        if (Data.PendingHoldings.Count == 0)
        {
            await bus.Publish(new ValuationsCalculatedEvent(correlationId, pipelineMode, asOfDate));
            
            logger.LogInformation("Calculate valuations saga {CorrelationKey} complete", Data.CorrelationKey);
            MarkAsComplete();
        }
    }
}

public class CalculateValuationsSagaData : SagaData
{
    public Guid CorrelationId { get; set; }
    public DateOnly AsOfDate { get; set; }
    
    public string CorrelationKey => $"{CorrelationId:N}:{AsOfDate}";
    public HashSet<int> PendingHoldings { get; set; } = new();
}