using Hoard.Core.Application;
using Hoard.Core.Application.Quotes;
using Hoard.Messages.Quotes;
using Microsoft.Extensions.Logging;
using Rebus.Bus;
using Rebus.Handlers;
using Rebus.Sagas;

namespace Hoard.Bus.Quotes;

public class RefreshQuotesSaga(
    IMediator mediator,
    IBus bus,
    ILogger<RefreshQuotesSaga> logger) :
    Saga<RqSagaData>,
    IAmInitiatedBy<StartRefreshQuotesSagaCommand>,
    IHandleMessages<QuoteBatchRefreshedEvent>
{
    private const int BatchSize = 10;

    protected override void CorrelateMessages(ICorrelationConfig<RqSagaData> cfg)
    {
        cfg.Correlate<StartRefreshQuotesSagaCommand>(m => m.QuotesRunId, d => d.QuotesRunId);
        cfg.Correlate<QuoteBatchRefreshedEvent>(m => m.QuotesRunId, d => d.QuotesRunId);
    }

    public async Task Handle(StartRefreshQuotesSagaCommand message)
    {
        Data.QuotesRunId = message.QuotesRunId;

        var instrumentIds = await mediator.QueryAsync<GetInstrumentsForQuoteRefreshQuery, IReadOnlyList<int>>(
            new GetInstrumentsForQuoteRefreshQuery());

        logger.LogInformation("Started refresh quotes saga {QuotesRunId} for {Count} instruments",
            Data.QuotesRunId, instrumentIds.Count);

        if (instrumentIds.Count == 0)
        {
            MarkAsComplete();
            await bus.Publish(new QuotesRefreshedEvent(Data.QuotesRunId));
            return;
        }

        var batchCount = (instrumentIds.Count + BatchSize - 1) / BatchSize;
        Data.PendingBatches = Enumerable.Range(0, batchCount).ToHashSet();

        await mediator.SendAsync(new DispatchRefreshQuotesCommand(message.QuotesRunId, instrumentIds));
    }

    public async Task Handle(QuoteBatchRefreshedEvent message)
    {
        Data.PendingBatches.Remove(message.BatchIndex);
        if (Data.PendingBatches.Count == 0)
        {
            logger.LogInformation("Quotes refresh saga {QuotesRunId} complete", Data.QuotesRunId);
            MarkAsComplete();
            await bus.Publish(new QuotesRefreshedEvent(Data.QuotesRunId));
        }
    }
}

public class RqSagaData : SagaData
{
    public Guid QuotesRunId { get; set; }
    public HashSet<int> PendingBatches { get; set; } = new();
}
