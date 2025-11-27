using Hoard.Core.Application;
using Hoard.Core.Application.Prices;
using Hoard.Messages;
using Hoard.Messages.Prices;
using Microsoft.Extensions.Logging;
using Rebus.Bus;
using Rebus.Handlers;
using Rebus.Sagas;

namespace Hoard.Bus.Handlers.Prices;

public class RefreshPricesSaga(
    IMediator mediator,
    IBus bus,
    ILogger<RefreshPricesSaga> logger) :
    Saga<RefreshPricesSagaData>,
    IAmInitiatedBy<StartRefreshPricesSagaCommand>,
    IHandleMessages<PriceRefreshedEvent>
{
    protected override void CorrelateMessages(ICorrelationConfig<RefreshPricesSagaData> cfg)
    {
        cfg.Correlate<StartRefreshPricesSagaCommand>(m => m.CorrelationId, d => d.CorrelationId);
        cfg.Correlate<PriceRefreshedEvent>(m => m.CorrelationId, d => d.CorrelationId);
    }

    public async Task Handle(StartRefreshPricesSagaCommand message)
    {
        var (correlationId, pipelineMode, instrumentId, startDate, endDate) = message;
        
        Data.CorrelationId = correlationId;

        var instrumentIds = await mediator.QueryAsync<GetInstrumentsForRefreshQuery, IReadOnlyList<int>>(
            new GetInstrumentsForRefreshQuery(instrumentId));

        logger.LogInformation("Started refresh prices saga {CorrelationId} for {Count} instruments",
            Data.CorrelationId, instrumentIds.Count);
        
        Data.PendingInstruments = instrumentIds.ToHashSet();
        
        await mediator.SendAsync(new DispatchRefreshPricesCommand(correlationId, pipelineMode, instrumentIds, startDate, endDate));
    }
    
    public async Task Handle(PriceRefreshedEvent message)
    {
        Data.PendingInstruments.Remove(message.InstrumentId);
        if (Data.PendingInstruments.Count == 0)
        {
            logger.LogInformation("Price refresh saga {CorrelationId} complete", Data.CorrelationId);
            MarkAsComplete();
        }

        await bus.Publish(new PricesRefreshedEvent(Data.CorrelationId, Data.PipelineMode));
    }
}

public class RefreshPricesSagaData : SagaData
{
    public Guid CorrelationId { get; set; }
    public PipelineMode PipelineMode { get; set; }
    public HashSet<int> PendingInstruments { get; set; } = new();
}