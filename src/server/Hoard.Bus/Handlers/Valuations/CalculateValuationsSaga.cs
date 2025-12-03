using Hoard.Core.Application;
using Hoard.Core.Application.Valuations;
using Hoard.Core.Extensions;
using Hoard.Messages;
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
        var (correlationId, pipelineMode, instrumentId, nullableAsOfDate) = message;
        
        Data.CorrelationId = correlationId;
        Data.PipelineMode = pipelineMode;

        var asOfDate = nullableAsOfDate.OrToday();
        Data.AsOfDate = asOfDate;

        var instrumentIds = await mediator.QueryAsync<GetInstrumentsForValuationQuery, IReadOnlyList<int>>(
            new GetInstrumentsForValuationQuery(asOfDate, instrumentId));
        
        logger.LogInformation("Started calculate valuations saga {CorrelationKey} for {Count} holdings",
            Data.CorrelationKey, instrumentIds.Count);
        
        if (instrumentIds.Count == 0)
        {
            MarkAsComplete();
            return;
        }
        
        Data.PendingHoldings = instrumentIds.ToHashSet();
        
        await mediator.SendAsync(new DispatchHoldingsValuationsCommand(correlationId, pipelineMode, instrumentIds, asOfDate));
    }

    public async Task Handle(HoldingValuationCalculatedEvent message)
    {
        Data.PendingHoldings.Remove(message.InstrumentId);
        if (Data.PendingHoldings.Count == 0)
        {
            await CompleteSaga();
        }
    }

    private async Task CompleteSaga()
    {
        await bus.Publish(new ValuationsCalculatedEvent());
            
        logger.LogInformation("Calculate valuations saga {CorrelationKey} complete", Data.CorrelationKey);
        MarkAsComplete();
    }
}

public class CalculateValuationsSagaData : SagaData
{
    public Guid CorrelationId { get; set; }
    public PipelineMode PipelineMode { get; set; }
    public DateOnly AsOfDate { get; set; }
    
    public string CorrelationKey => $"{CorrelationId:N}:{AsOfDate}";
    public HashSet<int> PendingHoldings { get; set; } = new();
}