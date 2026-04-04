using Hoard.Bus.Quotes;
using Hoard.Core.Application;
using Hoard.Core.Application.Quotes;
using Hoard.Messages.Quotes;
using Microsoft.Extensions.Logging;
using Moq;
using Rebus.Bus;

namespace Hoard.Test.Unit.Bus.Quotes;

public class RefreshQuotesSagaTests
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly Mock<IBus> _bus = new();
    private readonly RefreshQuotesSaga _saga;

    public RefreshQuotesSagaTests()
    {
        _saga = new RefreshQuotesSaga(_mediator.Object, _bus.Object, Mock.Of<ILogger<RefreshQuotesSaga>>())
        {
            Data = new RqSagaData()
        };
    }

    [Fact]
    public async Task Handle_Start_WithInstruments_CreatesBatchesAndDispatches()
    {
        var runId = Guid.NewGuid();
        var instrumentIds = Enumerable.Range(1, 25).ToList();

        _mediator.Setup(m => m.QueryAsync<GetInstrumentsForQuoteRefreshQuery, IReadOnlyList<int>>(
                It.IsAny<GetInstrumentsForQuoteRefreshQuery>(), default))
            .ReturnsAsync(instrumentIds);

        await _saga.Handle(new StartRefreshQuotesSagaCommand(runId));

        Assert.Equal(runId, _saga.Data.QuotesRunId);
        Assert.Equal(3, _saga.Data.PendingBatches.Count); // 25 instruments / 10 batch size = 3 batches

        _mediator.Verify(m => m.SendAsync(It.IsAny<DispatchRefreshQuotesCommand>(), default), Times.Once);
        _bus.Verify(b => b.Publish(It.IsAny<QuotesRefreshedEvent>(), null), Times.Never);
    }

    [Fact]
    public async Task Handle_Start_WithNoInstruments_CompletesImmediately()
    {
        var runId = Guid.NewGuid();

        _mediator.Setup(m => m.QueryAsync<GetInstrumentsForQuoteRefreshQuery, IReadOnlyList<int>>(
                It.IsAny<GetInstrumentsForQuoteRefreshQuery>(), default))
            .ReturnsAsync(new List<int>());

        await _saga.Handle(new StartRefreshQuotesSagaCommand(runId));

        _bus.Verify(b => b.Publish(It.Is<QuotesRefreshedEvent>(e =>
            e.QuotesRunId == runId), null), Times.Once);
    }

    [Fact]
    public async Task Handle_BatchRefreshed_RemovesPendingBatch()
    {
        var runId = Guid.NewGuid();
        _saga.Data.QuotesRunId = runId;
        _saga.Data.PendingBatches = new HashSet<int> { 0, 1, 2 };

        await _saga.Handle(new QuoteBatchRefreshedEvent(runId, 0));

        Assert.Equal(2, _saga.Data.PendingBatches.Count);
        Assert.DoesNotContain(0, _saga.Data.PendingBatches);
        _bus.Verify(b => b.Publish(It.IsAny<QuotesRefreshedEvent>(), null), Times.Never);
    }

    [Fact]
    public async Task Handle_LastBatchRefreshed_PublishesCompletionEvent()
    {
        var runId = Guid.NewGuid();
        _saga.Data.QuotesRunId = runId;
        _saga.Data.PendingBatches = new HashSet<int> { 2 };

        await _saga.Handle(new QuoteBatchRefreshedEvent(runId, 2));

        Assert.Empty(_saga.Data.PendingBatches);
        _bus.Verify(b => b.Publish(It.Is<QuotesRefreshedEvent>(e =>
            e.QuotesRunId == runId), null), Times.Once);
    }
}
