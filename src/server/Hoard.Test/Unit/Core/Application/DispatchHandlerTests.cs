using Hoard.Core.Application.Analysis;
using Hoard.Core.Application.Holdings;
using Hoard.Core.Application.News;
using Hoard.Core.Application.Performance;
using Hoard.Core.Application.Prices;
using Hoard.Core.Application.Quotes;
using Hoard.Core.Application.Snapshots;
using Hoard.Core.Application.Valuations;
using Hoard.Messages;
using Hoard.Messages.Analysis;
using Hoard.Messages.Holdings;
using Hoard.Messages.News;
using Hoard.Messages.Performance;
using Hoard.Messages.Prices;
using Hoard.Messages.Quotes;
using Hoard.Messages.Snapshots;
using Hoard.Messages.Valuations;
using Moq;
using Rebus.Bus;

namespace Hoard.Test.Unit.Core.Application;

public class DispatchRefreshPricesHandlerTests
{
    [Fact]
    public async Task HandleAsync_DefersOneBusCommandPerInstrument()
    {
        var bus = new Mock<IBus>();
        var handler = new DispatchRefreshPricesHandler(bus.Object, TimeProvider.System);
        var runId = Guid.NewGuid();

        await handler.HandleAsync(new DispatchRefreshPricesCommand(
            runId, PipelineMode.CloseOfDay, [1, 2, 3],
            new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 31)));

        bus.Verify(b => b.DeferLocal(It.IsAny<TimeSpan>(),
            It.IsAny<RefreshPricesBatchBusCommand>(), null), Times.Exactly(3));
    }

    [Fact]
    public async Task HandleAsync_EmptyInstrumentIds_SendsNothing()
    {
        var bus = new Mock<IBus>();
        var handler = new DispatchRefreshPricesHandler(bus.Object, TimeProvider.System);

        await handler.HandleAsync(new DispatchRefreshPricesCommand(
            Guid.NewGuid(), PipelineMode.CloseOfDay, [],
            new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 31)));

        bus.Verify(b => b.DeferLocal(It.IsAny<TimeSpan>(),
            It.IsAny<RefreshPricesBatchBusCommand>(), null), Times.Never);
    }
}

public class DispatchRefreshNewsHandlerTests
{
    [Fact]
    public async Task HandleAsync_DefersOneBusCommandPerInstrument()
    {
        var bus = new Mock<IBus>();
        var handler = new DispatchRefreshNewsHandler(bus.Object);
        var runId = Guid.NewGuid();

        await handler.HandleAsync(new DispatchRefreshNewsCommand(runId, PipelineMode.CloseOfDay, [1, 2]));

        bus.Verify(b => b.DeferLocal(It.IsAny<TimeSpan>(),
            It.IsAny<RefreshNewsBatchBusCommand>(), null), Times.Exactly(2));
    }
}

public class DispatchRefreshQuotesHandlerTests
{
    [Fact]
    public async Task HandleAsync_DefersCommandsInBatches()
    {
        var bus = new Mock<IBus>();
        var handler = new DispatchRefreshQuotesHandler(bus.Object);
        var instrumentIds = Enumerable.Range(1, 25).ToList();

        await handler.HandleAsync(new DispatchRefreshQuotesCommand(Guid.NewGuid(), instrumentIds));

        // 25 instruments / batch size 10 = 3 batches
        bus.Verify(b => b.DeferLocal(It.IsAny<TimeSpan>(),
            It.IsAny<RefreshQuotesBatchBusCommand>(), null), Times.Exactly(3));
    }
}

public class DispatchCalculateHoldingValuationsHandlerTests
{
    [Fact]
    public async Task HandleAsync_SendsOneBusCommandPerInstrument()
    {
        var bus = new Mock<IBus>();
        var handler = new DispatchHoldingsValuationHandler(bus.Object);
        var runId = Guid.NewGuid();

        await handler.HandleAsync(new DispatchCalculateHoldingValuationsCommand(
            runId, PipelineMode.CloseOfDay, [1, 2], new DateOnly(2024, 1, 15)));

        bus.Verify(b => b.SendLocal(
            It.IsAny<CalculateHoldingValuationsBusCommand>(), null), Times.Exactly(2));
    }
}

public class DispatchCalculatePortfolioValuationHandlerTests
{
    [Fact]
    public async Task HandleAsync_SendsOneBusCommandPerPortfolio()
    {
        var bus = new Mock<IBus>();
        var handler = new DispatchCalculatePortfolioValuationHandler(bus.Object);
        var runId = Guid.NewGuid();

        await handler.HandleAsync(new DispatchCalculatePortfolioValuationCommand(
            runId, PipelineMode.CloseOfDay, [1, 2], new DateOnly(2024, 1, 15)));

        bus.Verify(b => b.SendLocal(
            It.IsAny<CalculatePortfolioValuationBusCommand>(), null), Times.Exactly(2));
    }
}

public class DispatchCalculatePositionPerformanceHandlerTests
{
    [Fact]
    public async Task HandleAsync_SendsOneBusCommandPerInstrument()
    {
        var bus = new Mock<IBus>();
        var handler = new DispatchCalculatePositionPerformanceHandler(bus.Object);

        await handler.HandleAsync(new DispatchCalculatePositionPerformanceCommand(
            Guid.NewGuid(), [1, 2, 3], PipelineMode.CloseOfDay));

        bus.Verify(b => b.SendLocal(
            It.IsAny<CalculatePositionPerformanceBusCommand>(), null), Times.Exactly(3));
    }
}

public class DispatchCalculatePortfolioPerformanceHandlerTests
{
    [Fact]
    public async Task HandleAsync_SendsOneBusCommandPerPortfolio()
    {
        var bus = new Mock<IBus>();
        var handler = new DispatchCalculatePortfolioPerformanceHandler(bus.Object);

        await handler.HandleAsync(new DispatchCalculatePortfolioPerformanceCommand(
            Guid.NewGuid(), [1, 2], PipelineMode.CloseOfDay));

        bus.Verify(b => b.SendLocal(
            It.IsAny<CalculatePortfolioPerformanceBusCommand>(), null), Times.Exactly(2));
    }
}

public class DispatchCalculateSnapshotHandlerTests
{
    [Fact]
    public async Task HandleAsync_SendsOneBusCommandPerPortfolio()
    {
        var bus = new Mock<IBus>();
        var handler = new DispatchCalculateSnapshotHandler(bus.Object);

        await handler.HandleAsync(new DispatchCalculateSnapshotCommand(
            Guid.NewGuid(), PipelineMode.CloseOfDay, [1, 2], 2024));

        bus.Verify(b => b.SendLocal(
            It.IsAny<CalculateSnapshotBusCommand>(), null), Times.Exactly(2));
    }
}

public class DispatchBackfillHoldingsHandlerTests
{
    [Fact]
    public async Task HandleAsync_SendsOneBusCommandPerDate()
    {
        var bus = new Mock<IBus>();
        var handler = new DispatchBackfillHoldingsHandler(bus.Object);
        var dates = new List<DateOnly>
        {
            new(2024, 1, 1), new(2024, 1, 2), new(2024, 1, 3)
        };

        await handler.HandleAsync(new DispatchBackfillHoldingsCommand(
            Guid.NewGuid(), PipelineMode.Backfill, dates));

        bus.Verify(b => b.SendLocal(
            It.IsAny<CalculateHoldingsBusCommand>(), null), Times.Exactly(3));
    }
}

public class DispatchBackfillValuationsHandlerTests
{
    [Fact]
    public async Task HandleAsync_SendsOneBusCommandPerDate()
    {
        var bus = new Mock<IBus>();
        var handler = new DispatchBackfillValuationsHandler(bus.Object);
        var dates = new List<DateOnly> { new(2024, 1, 1), new(2024, 1, 2) };

        await handler.HandleAsync(new DispatchBackfillValuationsCommand(
            Guid.NewGuid(), PipelineMode.Backfill, null, dates));

        bus.Verify(b => b.SendLocal(
            It.IsAny<StartCalculateValuationsSagaCommand>(), null), Times.Exactly(2));
    }
}

public class DispatchBackfillSnapshotsHandlerTests
{
    [Fact]
    public async Task HandleAsync_SendsOneBusCommandPerYear()
    {
        var bus = new Mock<IBus>();
        var handler = new DispatchBackfillSnapshotsHandler(bus.Object);

        await handler.HandleAsync(new DispatchBackfillSnapshotsCommand(
            Guid.NewGuid(), PipelineMode.Backfill, null, [2023, 2024]));

        bus.Verify(b => b.SendLocal(
            It.IsAny<StartCalculateSnapshotsSagaCommand>(), null), Times.Exactly(2));
    }
}

public class DispatchAnalyseInstrumentsHandlerTests
{
    [Fact]
    public async Task HandleAsync_SendsOneBusCommandPerInstrument()
    {
        var bus = new Mock<IBus>();
        var handler = new DispatchAnalyseInstrumentsHandler(bus.Object);

        await handler.HandleAsync(new DispatchAnalyseInstrumentsCommand(
            Guid.NewGuid(), PipelineMode.CloseOfDay, [1, 2], new DateOnly(2024, 1, 15)));

        bus.Verify(b => b.SendLocal(
            It.IsAny<AnalyseInstrumentBatchBusCommand>(), null), Times.Exactly(2));
    }
}
