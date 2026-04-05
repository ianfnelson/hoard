using Hoard.Bus.Holdings;
using Hoard.Core.Application;
using Hoard.Core.Application.Holdings;
using Hoard.Core.Application.Transactions;
using Hoard.Messages;
using Hoard.Messages.Holdings;
using Microsoft.Extensions.Logging;
using Moq;
using Rebus.Bus;

namespace Hoard.Test.Unit.Bus.Holdings;

public class BackfillHoldingsSagaTests
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly Mock<IBus> _bus = new();
    private readonly BackfillHoldingsSaga _saga;

    public BackfillHoldingsSagaTests()
    {
        _saga = new BackfillHoldingsSaga(_mediator.Object, _bus.Object, Mock.Of<ILogger<BackfillHoldingsSaga>>())
        {
            Data = new BfSagaData()
        };
    }

    [Fact]
    public async Task Handle_Start_QueriesDatesAndDispatchesBackfill()
    {
        var runId = Guid.NewGuid();
        var dates = new List<DateOnly>
        {
            new(2024, 6, 13), new(2024, 6, 14), new(2024, 6, 15)
        };

        _mediator.Setup(m => m.QueryAsync<GetDatesForBackfillQuery, IReadOnlyList<DateOnly>>(
                It.Is<GetDatesForBackfillQuery>(q =>
                    q.StartDate == new DateOnly(2024, 6, 13) && q.EndDate == new DateOnly(2024, 6, 15)), default))
            .ReturnsAsync(dates);

        await _saga.Handle(new StartBackfillHoldingsSagaCommand(runId, PipelineMode.Backfill, new DateOnly(2024, 6, 13), new DateOnly(2024, 6, 15)));

        Assert.Equal(runId, _saga.Data.HoldingsRunId);
        Assert.Equal(3, _saga.Data.PendingDates.Count);

        _mediator.Verify(m => m.SendAsync(It.Is<DispatchBackfillHoldingsCommand>(c =>
            c.HoldingsRunId == runId &&
            c.PipelineMode == PipelineMode.Backfill &&
            c.Dates.Count == 3), default), Times.Once);
    }

    [Fact]
    public async Task Handle_HoldingsCalculated_RemovesPendingDate()
    {
        var runId = Guid.NewGuid();
        _saga.Data.HoldingsRunId = runId;
        _saga.Data.PipelineMode = PipelineMode.Backfill;
        _saga.Data.PendingDates = new HashSet<DateOnly> { new(2024, 6, 14), new(2024, 6, 15) };

        await _saga.Handle(new HoldingsCalculatedEvent(runId, PipelineMode.Backfill, new DateOnly(2024, 6, 14)));

        Assert.DoesNotContain(new DateOnly(2024, 6, 14), _saga.Data.PendingDates);
        Assert.Contains(new DateOnly(2024, 6, 15), _saga.Data.PendingDates);
        _bus.Verify(b => b.Publish(It.IsAny<HoldingsBackfilledEvent>(), null), Times.Never);
    }

    [Fact]
    public async Task Handle_LastHoldingsCalculated_PublishesCompletionEvent()
    {
        var runId = Guid.NewGuid();
        _saga.Data.HoldingsRunId = runId;
        _saga.Data.PipelineMode = PipelineMode.Backfill;
        _saga.Data.StartDate = new DateOnly(2024, 6, 13);
        _saga.Data.EndDate = new DateOnly(2024, 6, 15);
        _saga.Data.PendingDates = new HashSet<DateOnly> { new(2024, 6, 15) };

        await _saga.Handle(new HoldingsCalculatedEvent(runId, PipelineMode.Backfill, new DateOnly(2024, 6, 15)));

        Assert.Empty(_saga.Data.PendingDates);
        _bus.Verify(b => b.Publish(It.Is<HoldingsBackfilledEvent>(e =>
            e.HoldingsRunId == runId &&
            e.PipelineMode == PipelineMode.Backfill &&
            e.StartDate == new DateOnly(2024, 6, 13) &&
            e.EndDate == new DateOnly(2024, 6, 15)), null), Times.Once);
    }
}
