using Hoard.Bus.Valuations;
using Hoard.Core.Application;
using Hoard.Core.Application.Transactions;
using Hoard.Core.Application.Valuations;
using Hoard.Messages;
using Hoard.Messages.Valuations;
using Microsoft.Extensions.Logging;
using Moq;
using Rebus.Bus;

namespace Hoard.Test.Unit.Bus.Valuations;

public class BackfillValuationsSagaTests
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly Mock<IBus> _bus = new();
    private readonly BackfillValuationsSaga _saga;

    public BackfillValuationsSagaTests()
    {
        _saga = new BackfillValuationsSaga(_mediator.Object, _bus.Object, Mock.Of<ILogger<BackfillValuationsSaga>>())
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
                It.IsAny<GetDatesForBackfillQuery>(), default))
            .ReturnsAsync(dates);

        await _saga.Handle(new StartBackfillValuationsSagaCommand(runId, PipelineMode.Backfill, 42, new DateOnly(2024, 6, 13), new DateOnly(2024, 6, 15)));

        Assert.Equal(runId, _saga.Data.ValuationsRunId);
        Assert.Equal(PipelineMode.Backfill, _saga.Data.PipelineMode);
        Assert.Equal(42, _saga.Data.InstrumentId);
        Assert.Equal(new DateOnly(2024, 6, 13), _saga.Data.StartDate);
        Assert.Equal(new DateOnly(2024, 6, 15), _saga.Data.EndDate);
        Assert.Equal(3, _saga.Data.PendingDates.Count);

        _mediator.Verify(m => m.SendAsync(It.IsAny<DispatchBackfillValuationsCommand>(), default), Times.Once);
    }

    [Fact]
    public async Task Handle_ValuationsCalculated_RemovesPendingDate()
    {
        var runId = Guid.NewGuid();
        _saga.Data.ValuationsRunId = runId;
        _saga.Data.PipelineMode = PipelineMode.Backfill;
        _saga.Data.PendingDates = new HashSet<DateOnly> { new(2024, 6, 14), new(2024, 6, 15) };

        await _saga.Handle(new ValuationsCalculatedEvent(runId, PipelineMode.Backfill, new DateOnly(2024, 6, 14)));

        Assert.Single(_saga.Data.PendingDates);
        Assert.Contains(new DateOnly(2024, 6, 15), _saga.Data.PendingDates);
        _bus.Verify(b => b.Publish(It.IsAny<ValuationsBackfilledEvent>(), null), Times.Never);
    }

    [Fact]
    public async Task Handle_LastValuationsCalculated_PublishesCompletionEvent()
    {
        var runId = Guid.NewGuid();
        _saga.Data.ValuationsRunId = runId;
        _saga.Data.PipelineMode = PipelineMode.Backfill;
        _saga.Data.InstrumentId = 42;
        _saga.Data.StartDate = new DateOnly(2024, 6, 13);
        _saga.Data.EndDate = new DateOnly(2024, 6, 15);
        _saga.Data.PendingDates = new HashSet<DateOnly> { new(2024, 6, 15) };

        await _saga.Handle(new ValuationsCalculatedEvent(runId, PipelineMode.Backfill, new DateOnly(2024, 6, 15)));

        Assert.Empty(_saga.Data.PendingDates);
        _bus.Verify(b => b.Publish(It.Is<ValuationsBackfilledEvent>(e =>
            e.ValuationsRunId == runId &&
            e.PipelineMode == PipelineMode.Backfill &&
            e.InstrumentId == 42 &&
            e.StartDate == new DateOnly(2024, 6, 13) &&
            e.EndDate == new DateOnly(2024, 6, 15)), null), Times.Once);
    }
}
