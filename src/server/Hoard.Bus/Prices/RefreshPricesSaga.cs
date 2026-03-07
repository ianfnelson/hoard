using Hoard.Core.Application;
using Hoard.Core.Application.Prices;
using Hoard.Messages;
using Hoard.Messages.Prices;
using Microsoft.Extensions.Logging;
using Rebus.Bus;
using Rebus.Handlers;
using Rebus.Sagas;

namespace Hoard.Bus.Prices;

public class RefreshPricesSaga(
    IMediator mediator,
    IBus bus,
    ILogger<RefreshPricesSaga> logger) :
    Saga<RpSagaData>,
    IAmInitiatedBy<StartRefreshPricesSagaCommand>,
    IHandleMessages<PriceRefreshedEvent>
{
    protected override void CorrelateMessages(ICorrelationConfig<RpSagaData> cfg)
    {
        cfg.Correlate<StartRefreshPricesSagaCommand>(m => m.PricesRunId, d => d.PricesRunId);
        cfg.Correlate<PriceRefreshedEvent>(m => m.PricesRunId, d => d.PricesRunId);
    }

    public async Task Handle(StartRefreshPricesSagaCommand message)
    {
        var (pricesRunId, pipelineMode, instrumentId, startDate, endDate) = message;

        Data.PricesRunId = pricesRunId;
        Data.PipelineMode = pipelineMode;

        var instrumentIds = await mediator.QueryAsync<GetInstrumentsForRefreshQuery, IReadOnlyList<int>>(
            new GetInstrumentsForRefreshQuery(instrumentId, pipelineMode));

        logger.LogInformation("Started refresh prices saga {PricesRunId} for {Count} instruments",
            Data.PricesRunId, instrumentIds.Count);

        Data.PendingInstruments = instrumentIds.ToHashSet();

        await mediator.SendAsync(new DispatchRefreshPricesCommand(pricesRunId, pipelineMode, instrumentIds, startDate, endDate));

        // If there are no instruments to process, complete immediately
        if (instrumentIds.Count == 0)
        {
            logger.LogInformation("Price refresh saga {PricesRunId} complete (no instruments to process)", Data.PricesRunId);
            MarkAsComplete();
            await bus.Publish(new PricesRefreshedEvent(Data.PricesRunId, Data.PipelineMode));
        }
    }
    
    public async Task Handle(PriceRefreshedEvent message)
    {
        Data.PendingInstruments.Remove(message.InstrumentId);
        if (Data.PendingInstruments.Count == 0)
        {
            logger.LogInformation("Price refresh saga {PricesRunId} complete", Data.PricesRunId);
            MarkAsComplete();
            await bus.Publish(new PricesRefreshedEvent(Data.PricesRunId, Data.PipelineMode));
        }
    }
}

public class RpSagaData : SagaData
{
    public Guid PricesRunId { get; set; }
    public PipelineMode PipelineMode { get; set; }
    public HashSet<int> PendingInstruments { get; set; } = new();
}
