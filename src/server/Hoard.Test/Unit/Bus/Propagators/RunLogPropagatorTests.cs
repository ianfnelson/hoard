using Hoard.Bus.RunLog;
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
using Moq;

namespace Hoard.Test.Unit.Bus.Propagators;

public class RunLogPropagatorTests
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly RunLogPropagator _propagator;

    public RunLogPropagatorTests()
    {
        _propagator = new RunLogPropagator(_mediator.Object);
    }

    [Fact]
    public async Task Handle_CloseOfDayCompleted_CompletesRunWithCloseOfDayType()
    {
        var runId = Guid.NewGuid();
        await _propagator.Handle(new CloseOfDayCompletedEvent(runId, new DateOnly(2024, 6, 15), PipelineMode.CloseOfDay));

        VerifyCompleteRun(runId, RunType.CloseOfDay);
    }

    [Fact]
    public async Task Handle_PricesRefreshed_CloseOfDay_CompletesWithRefreshPricesType()
    {
        var runId = Guid.NewGuid();
        await _propagator.Handle(new PricesRefreshedEvent(runId, PipelineMode.CloseOfDay));

        VerifyCompleteRun(runId, RunType.RefreshPrices);
    }

    [Fact]
    public async Task Handle_PricesRefreshed_DaytimeReactive_CompletesWithRefreshPricesType()
    {
        var runId = Guid.NewGuid();
        await _propagator.Handle(new PricesRefreshedEvent(runId, PipelineMode.DaytimeReactive));

        VerifyCompleteRun(runId, RunType.RefreshPrices);
    }

    [Fact]
    public async Task Handle_PricesRefreshed_Backfill_CompletesWithBackfillPricesType()
    {
        var runId = Guid.NewGuid();
        await _propagator.Handle(new PricesRefreshedEvent(runId, PipelineMode.Backfill));

        VerifyCompleteRun(runId, RunType.BackfillPrices);
    }

    [Fact]
    public async Task Handle_HoldingsBackfilled_CompletesWithBackfillHoldingsType()
    {
        var runId = Guid.NewGuid();
        await _propagator.Handle(new HoldingsBackfilledEvent(runId, PipelineMode.Backfill, new DateOnly(2024, 1, 1), new DateOnly(2024, 6, 15)));

        VerifyCompleteRun(runId, RunType.BackfillHoldings);
    }

    [Fact]
    public async Task Handle_HoldingsCalculated_CompletesWithCalculateHoldingsType()
    {
        var runId = Guid.NewGuid();
        await _propagator.Handle(new HoldingsCalculatedEvent(runId, PipelineMode.CloseOfDay, new DateOnly(2024, 6, 15)));

        VerifyCompleteRun(runId, RunType.CalculateHoldings);
    }

    [Fact]
    public async Task Handle_PositionsCalculated_CompletesWithCalculatePositionsType()
    {
        var runId = Guid.NewGuid();
        await _propagator.Handle(new PositionsCalculatedEvent(runId, PipelineMode.CloseOfDay));

        VerifyCompleteRun(runId, RunType.CalculatePositions);
    }

    [Fact]
    public async Task Handle_ValuationsBackfilled_CompletesWithBackfillValuationsType()
    {
        var runId = Guid.NewGuid();
        await _propagator.Handle(new ValuationsBackfilledEvent(runId, PipelineMode.Backfill, null, new DateOnly(2024, 1, 1), new DateOnly(2024, 6, 15)));

        VerifyCompleteRun(runId, RunType.BackfillValuations);
    }

    [Fact]
    public async Task Handle_ValuationsCalculated_CompletesWithCalculateValuationsType()
    {
        var runId = Guid.NewGuid();
        await _propagator.Handle(new ValuationsCalculatedEvent(runId, PipelineMode.CloseOfDay, new DateOnly(2024, 6, 15)));

        VerifyCompleteRun(runId, RunType.CalculateValuations);
    }

    [Fact]
    public async Task Handle_PerformanceCalculated_CompletesWithCalculatePerformanceType()
    {
        var runId = Guid.NewGuid();
        await _propagator.Handle(new PerformanceCalculatedEvent(runId, PipelineMode.CloseOfDay));

        VerifyCompleteRun(runId, RunType.CalculatePerformance);
    }

    [Fact]
    public async Task Handle_SnapshotsBackfilled_CompletesWithBackfillSnapshotsType()
    {
        var runId = Guid.NewGuid();
        await _propagator.Handle(new SnapshotsBackfilledEvent(runId, PipelineMode.Backfill));

        VerifyCompleteRun(runId, RunType.BackfillSnapshots);
    }

    [Fact]
    public async Task Handle_SnapshotsCalculated_CompletesWithCalculateSnapshotsType()
    {
        var runId = Guid.NewGuid();
        await _propagator.Handle(new SnapshotsCalculatedEvent(runId, PipelineMode.CloseOfDay, 2024));

        VerifyCompleteRun(runId, RunType.CalculateSnapshots);
    }

    [Fact]
    public async Task Handle_InstrumentsAnalysed_CompletesWithAnalyseInstrumentsType()
    {
        var runId = Guid.NewGuid();
        await _propagator.Handle(new InstrumentsAnalysedEvent(runId, PipelineMode.CloseOfDay));

        VerifyCompleteRun(runId, RunType.AnalyseInstruments);
    }

    [Fact]
    public async Task Handle_AllNewsRefreshed_CompletesWithRefreshNewsType()
    {
        var runId = Guid.NewGuid();
        await _propagator.Handle(new AllNewsRefreshedEvent(runId, PipelineMode.CloseOfDay));

        VerifyCompleteRun(runId, RunType.RefreshNews);
    }

    [Fact]
    public async Task Handle_QuotesRefreshed_CompletesWithRefreshQuotesType()
    {
        var runId = Guid.NewGuid();
        await _propagator.Handle(new QuotesRefreshedEvent(runId));

        VerifyCompleteRun(runId, RunType.RefreshQuotes);
    }

    private void VerifyCompleteRun(Guid runId, int expectedRunTypeId)
    {
        _mediator.Verify(m => m.SendAsync(
            It.Is<CompleteRunCommand>(c =>
                c.RunId == runId && c.RunTypeId == expectedRunTypeId),
            default), Times.Once);
    }
}
