using Hoard.Core.Application;
using Hoard.Core.Application.Prices;
using Hoard.Messages.Prices;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;
using Rebus.Sagas;

namespace Hoard.Bus.Handlers.Prices;

public class BackfillPricesSaga(
    IMediator mediator,
    ILogger<BackfillPricesSaga> logger) :
    Saga<BackfillPricesSagaData>,
    IAmInitiatedBy<StartBackfillPricesSagaCommand>,
    IHandleMessages<PriceRefreshedEvent>
{
    protected override void CorrelateMessages(ICorrelationConfig<BackfillPricesSagaData> cfg)
    {
        cfg.Correlate<StartBackfillPricesSagaCommand>(m => m.CorrelationId, d => d.CorrelationId);
        cfg.Correlate<PriceRefreshedEvent>(m => m.CorrelationId, d => d.CorrelationId);
    }

    public async Task Handle(StartBackfillPricesSagaCommand message)
    {
        Data.CorrelationId = message.CorrelationId;

        var instrumentIds = await mediator.QueryAsync<GetInstrumentsForBackfillQuery, IReadOnlyList<int>>(
            new GetInstrumentsForBackfillQuery(message.InstrumentId));

        logger.LogInformation("Started backfill prices saga {CorrelationId} for {Count} instruments",
            Data.CorrelationId, instrumentIds.Count);
        
        Data.PendingInstruments = instrumentIds.ToHashSet();
        
        await mediator.SendAsync(new DispatchBackfillPricesCommand(message.CorrelationId, instrumentIds, message.StartDate, message.EndDate));
    }
    
    public Task Handle(PriceRefreshedEvent message)
    {
        Data.PendingInstruments.Remove(message.InstrumentId);
        if (Data.PendingInstruments.Count == 0)
        {
            logger.LogInformation("Price backfill saga {CorrelationId} complete", Data.CorrelationId);
            MarkAsComplete();
        }

        return Task.CompletedTask;
    }
}

public class BackfillPricesSagaData : SagaData
{
    public Guid CorrelationId { get; set; }
    public HashSet<int> PendingInstruments { get; set; } = new();
}