using Hoard.Core.Application;
using Hoard.Core.Application.Valuations;
using Hoard.Core.Extensions;
using Hoard.Messages.Valuations;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;
using Rebus.Sagas;

namespace Hoard.Bus.Handlers.Valuations;

public class CalculateValuationsSaga(ILogger<CalculateValuationsSaga> logger, IMediator mediator)
    :
        Saga<CalculateValuationsSagaData>,
        IAmInitiatedBy<StartCalculateValuationsSagaCommand>,
        IHandleMessages<HoldingValuationCalculatedEvent>
{
    protected override void CorrelateMessages(ICorrelationConfig<CalculateValuationsSagaData> cfg)
    {
        cfg.Correlate<StartCalculateValuationsSagaCommand>(
            m => $"{m.CorrelationId:N}:{m.AsOfDate}",
            d => d.CorrelationKey);

        cfg.Correlate<HoldingValuationCalculatedEvent>(
            m => $"{m.CorrelationId:N}:{m.AsOfDate}",
            d => d.CorrelationKey);
    }

    public async Task Handle(StartCalculateValuationsSagaCommand message)
    {
        var (correlationId, nullableAsOfDate, isBackfill) = message;
        
        Data.CorrelationId = correlationId;

        var asOfDate = nullableAsOfDate.OrToday();
        Data.AsOfDate = asOfDate;

        var holdingIds = await mediator.QueryAsync<GetHoldingsForValuationQuery, IReadOnlyList<int>>(
            new GetHoldingsForValuationQuery(asOfDate));
        
        if (holdingIds.Count == 0)
        {
            MarkAsComplete();
            return;
        }

        logger.LogInformation("Started calculate valuations saga {CorrelationKey} for {Count} holdings",
            Data.CorrelationKey, holdingIds.Count);

        Data.PendingHoldings = holdingIds.ToHashSet();
        
        await mediator.SendAsync(new DispatchHoldingsValuationCommand(correlationId, holdingIds, isBackfill));
    }

    public async Task Handle(HoldingValuationCalculatedEvent message)
    {
        var (correlationId, holdingId, asOfDate, isBackfill) = message;
        
        Data.PendingHoldings.Remove(holdingId);
        if (Data.PendingHoldings.Count == 0)
        {
            await mediator.SendAsync(
                new ProcessCalculatePortfolioValuationsCommand(correlationId, asOfDate, isBackfill));
            
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