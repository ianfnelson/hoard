using Hoard.Core.Application;
using Hoard.Core.Application.News;
using Hoard.Messages;
using Hoard.Messages.News;
using Microsoft.Extensions.Logging;
using Rebus.Bus;
using Rebus.Handlers;
using Rebus.Sagas;

namespace Hoard.Bus.News;

public class RefreshNewsSaga(
    IMediator mediator,
    IBus bus,
    ILogger<RefreshNewsSaga> logger) :
    Saga<RnSagaData>,
    IAmInitiatedBy<StartRefreshNewsSagaCommand>,
    IHandleMessages<NewsRefreshedEvent>
{
    protected override void CorrelateMessages(ICorrelationConfig<RnSagaData> cfg)
    {
        cfg.Correlate<StartRefreshNewsSagaCommand>(m => m.NewsRunId, d => d.NewsRunId);
        cfg.Correlate<NewsRefreshedEvent>(m => m.NewsRunId, d => d.NewsRunId);
    }

    public async Task Handle(StartRefreshNewsSagaCommand message)
    {
        var (newsRunId, pipelineMode, instrumentId) = message;

        Data.NewsRunId = newsRunId;
        Data.PipelineMode = pipelineMode;

        var instrumentIds = await mediator.QueryAsync<GetInstrumentsForNewsRefreshQuery, IReadOnlyList<int>>(
            new GetInstrumentsForNewsRefreshQuery(instrumentId));

        logger.LogInformation("Started refresh news saga {NewsRunId} for {Count} instruments",
            Data.NewsRunId, instrumentIds.Count);

        Data.PendingInstruments = instrumentIds.ToHashSet();

        await mediator.SendAsync(new DispatchRefreshNewsCommand(newsRunId, pipelineMode, instrumentIds));
    }

    public async Task Handle(NewsRefreshedEvent message)
    {
        Data.PendingInstruments.Remove(message.InstrumentId);
        if (Data.PendingInstruments.Count == 0)
        {
            logger.LogInformation("News refresh saga {NewsRunId} complete", Data.NewsRunId);
            MarkAsComplete();
            //await bus.Publish(new AllNewsRefreshedEvent(Data.NewsRunId, Data.PipelineMode));
        }
    }
}

public class RnSagaData : SagaData
{
    public Guid NewsRunId { get; set; }
    public PipelineMode PipelineMode { get; set; }
    public HashSet<int> PendingInstruments { get; set; } = new();
}
