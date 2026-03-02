using Hoard.Core.Application;
using Hoard.Core.Application.RunLog;
using Hoard.Core.Domain.Entities;
using Hoard.Messages;
using Hoard.Messages.Analysis;
using Hoard.Messages.Chrono;
using Hoard.Messages.Holdings;
using Hoard.Messages.News;
using Hoard.Messages.Performance;
using Hoard.Messages.Positions;
using Hoard.Messages.Prices;
using Hoard.Messages.Quotes;
using Hoard.Messages.Snapshots;
using Hoard.Messages.Valuations;
using Rebus.Handlers;

namespace Hoard.Bus.RunLog;

public class RunLogPropagator(IMediator mediator) :
    IHandleMessages<CloseOfDayCompletedEvent>,
    IHandleMessages<PricesRefreshedEvent>,
    IHandleMessages<HoldingsBackfilledEvent>,
    IHandleMessages<HoldingsCalculatedEvent>,
    IHandleMessages<PositionsCalculatedEvent>,
    IHandleMessages<ValuationsBackfilledEvent>,
    IHandleMessages<ValuationsCalculatedEvent>,
    IHandleMessages<PerformanceCalculatedEvent>,
    IHandleMessages<SnapshotsBackfilledEvent>,
    IHandleMessages<SnapshotsCalculatedEvent>,
    IHandleMessages<InstrumentsAnalysedEvent>,
    IHandleMessages<AllNewsRefreshedEvent>,
    IHandleMessages<QuotesRefreshedEvent>
{
    public Task Handle(CloseOfDayCompletedEvent message) =>
        Complete(message.ChronoRunId, RunType.CloseOfDay);

    public Task Handle(PricesRefreshedEvent message) =>
        Complete(message.PricesRunId, GetRunTypeForPricesRefreshedEvent(message.PipelineMode));

    private static int GetRunTypeForPricesRefreshedEvent(PipelineMode pipelineMode)
    {
        switch (pipelineMode)
        {
            case PipelineMode.Backfill:
                return RunType.BackfillPrices;
            case PipelineMode.DaytimeReactive:
            case PipelineMode.CloseOfDay:
                return RunType.RefreshPrices;
            default:
                throw new ArgumentOutOfRangeException(nameof(pipelineMode), pipelineMode, null);
        }
    }
    
    public Task Handle(HoldingsBackfilledEvent message) =>
        Complete(message.HoldingsRunId, RunType.BackfillHoldings);

    public Task Handle(HoldingsCalculatedEvent message) =>
        Complete(message.HoldingsRunId, RunType.CalculateHoldings);

    public Task Handle(PositionsCalculatedEvent message) =>
        Complete(message.PositionsRunId, RunType.CalculatePositions);

    public Task Handle(ValuationsBackfilledEvent message) =>
        Complete(message.ValuationsRunId, RunType.BackfillValuations);

    public Task Handle(ValuationsCalculatedEvent message) =>
        Complete(message.ValuationsRunId, RunType.CalculateValuations);

    public Task Handle(PerformanceCalculatedEvent message) =>
        Complete(message.PerformanceRunId, RunType.CalculatePerformance);

    public Task Handle(SnapshotsBackfilledEvent message) =>
        Complete(message.SnapshotsRunId, RunType.BackfillSnapshots);

    public Task Handle(SnapshotsCalculatedEvent message) =>
        Complete(message.SnapshotsRunId, RunType.CalculateSnapshots);

    public Task Handle(InstrumentsAnalysedEvent message) =>
        Complete(message.AnalysisRunId, RunType.AnalyseInstruments);

    public Task Handle(AllNewsRefreshedEvent message) =>
        Complete(message.NewsRunId, RunType.RefreshNews);

    public Task Handle(QuotesRefreshedEvent message) =>
        Complete(message.QuotesRunId, RunType.RefreshQuotes);

    private Task Complete(Guid runId, int runTypeId) =>
        mediator.SendAsync(new CompleteRunCommand(runId, runTypeId, DateTime.UtcNow));
}
