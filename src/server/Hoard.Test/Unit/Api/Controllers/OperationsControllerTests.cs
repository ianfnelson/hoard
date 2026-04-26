using Hoard.Api.Controllers;
using Hoard.Core;
using Hoard.Core.Application;
using Hoard.Core.Application.Analysis;
using Microsoft.Extensions.Time.Testing;
using Hoard.Core.Application.Chrono;
using Hoard.Core.Application.Holdings;
using Hoard.Core.Application.News;
using Hoard.Core.Application.Performance;
using Hoard.Core.Application.Positions;
using Hoard.Core.Application.Prices;
using Hoard.Core.Application.Quotes;
using Hoard.Core.Application.Snapshots;
using Hoard.Core.Application.Valuations;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Hoard.Test.Unit.Api.Controllers;

public class AnalysisOperationsControllerTests
{
    private static readonly DateOnly FixedToday = new(2024, 6, 15);

    private readonly Mock<IMediator> _mediator = new();
    private readonly FakeTimeProvider _timeProvider = new();

    public AnalysisOperationsControllerTests()
    {
        var ukNoon = new DateTime(FixedToday.Year, FixedToday.Month, FixedToday.Day, 12, 0, 0, DateTimeKind.Unspecified);
        var utc = TimeZoneInfo.ConvertTimeToUtc(ukNoon, DateOnlyHelper.UkTimeZone);
        _timeProvider.SetUtcNow(new DateTimeOffset(utc, TimeSpan.Zero));
    }

    [Fact]
    public async Task AnalyseInstruments_ReturnsAccepted()
    {
        var controller = new AnalysisOperationsController(_mediator.Object, _timeProvider);

        var result = await controller.AnalyseInstrumentsAsync(new AnalyseInstrumentsRequest());

        Assert.IsType<AcceptedResult>(result);
        _mediator.Verify(m => m.SendAsync(
            It.IsAny<TriggerAnalyseInstrumentsCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AnalyseInstruments_WithAsOfDate_UsesProvidedDate()
    {
        var date = new DateOnly(2024, 6, 15);
        var controller = new AnalysisOperationsController(_mediator.Object, _timeProvider);

        await controller.AnalyseInstrumentsAsync(new AnalyseInstrumentsRequest(date));

        _mediator.Verify(m => m.SendAsync(
            It.Is<TriggerAnalyseInstrumentsCommand>(c => c.AsOfDate == date),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AnalyseInstruments_WithoutAsOfDate_UsesToday()
    {
        var controller = new AnalysisOperationsController(_mediator.Object, _timeProvider);

        await controller.AnalyseInstrumentsAsync(new AnalyseInstrumentsRequest());

        _mediator.Verify(m => m.SendAsync(
            It.Is<TriggerAnalyseInstrumentsCommand>(c => c.AsOfDate == FixedToday),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}

public class ChronoOperationsControllerTests
{
    private readonly Mock<IMediator> _mediator = new();

    [Fact]
    public async Task TriggerCloseOfDay_ReturnsAccepted()
    {
        var controller = new ChronoOperationsController(_mediator.Object);
        var command = new TriggerCloseOfDayRunCommand(new DateOnly(2024, 1, 15));

        var result = await controller.TriggerCloseOfDay(command);

        Assert.IsType<AcceptedResult>(result);
        _mediator.Verify(m => m.SendAsync(command, It.IsAny<CancellationToken>()), Times.Once);
    }
}

public class HoldingsOperationsControllerTests
{
    private readonly Mock<IMediator> _mediator = new();

    [Fact]
    public async Task BackfillHoldings_ReturnsAccepted()
    {
        var controller = new HoldingsOperationsController(_mediator.Object);
        var command = new TriggerBackfillHoldingsCommand(new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 31));

        var result = await controller.BackfillHoldingsAsync(command);

        Assert.IsType<AcceptedResult>(result);
        _mediator.Verify(m => m.SendAsync(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CalculateHoldings_ReturnsAccepted()
    {
        var controller = new HoldingsOperationsController(_mediator.Object);
        var command = new TriggerCalculateHoldingsCommand(new DateOnly(2024, 1, 15));

        var result = await controller.CalculateHoldingsAsync(command);

        Assert.IsType<AcceptedResult>(result);
        _mediator.Verify(m => m.SendAsync(command, It.IsAny<CancellationToken>()), Times.Once);
    }
}

public class NewsOperationsControllerTests
{
    private readonly Mock<IMediator> _mediator = new();

    [Fact]
    public async Task RefreshNews_ReturnsAccepted()
    {
        var controller = new NewsOperationsController(_mediator.Object);
        var command = new TriggerRefreshNewsCommand(null);

        var result = await controller.RefreshNewsAsync(command);

        Assert.IsType<AcceptedResult>(result);
        _mediator.Verify(m => m.SendAsync(command, It.IsAny<CancellationToken>()), Times.Once);
    }
}

public class PerformanceOperationsControllerTests
{
    private readonly Mock<IMediator> _mediator = new();

    [Fact]
    public async Task CalculatePerformance_ReturnsAccepted()
    {
        var controller = new PerformanceOperationsController(_mediator.Object);
        var command = new TriggerCalculatePerformanceCommand(null);

        var result = await controller.CalculatePerformanceAsync(command);

        Assert.IsType<AcceptedResult>(result);
        _mediator.Verify(m => m.SendAsync(command, It.IsAny<CancellationToken>()), Times.Once);
    }
}

public class PositionsOperationsControllerTests
{
    private readonly Mock<IMediator> _mediator = new();

    [Fact]
    public async Task CalculatePositions_ReturnsAccepted()
    {
        var controller = new PositionsOperationsController(_mediator.Object);

        var result = await controller.CalculatePositionsAsync();

        Assert.IsType<AcceptedResult>(result);
        _mediator.Verify(m => m.SendAsync(
            It.IsAny<TriggerCalculatePositionsCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}

public class PricesOperationsControllerTests
{
    private readonly Mock<IMediator> _mediator = new();

    [Fact]
    public async Task BackfillPrices_ReturnsAccepted()
    {
        var controller = new PricesOperationsController(_mediator.Object);
        var command = new TriggerBackfillPricesCommand(null, new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 31));

        var result = await controller.BackfillPricesAsync(command);

        Assert.IsType<AcceptedResult>(result);
        _mediator.Verify(m => m.SendAsync(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RefreshPrices_ReturnsAccepted()
    {
        var controller = new PricesOperationsController(_mediator.Object);
        var command = new TriggerRefreshPricesCommand(null, null, null);

        var result = await controller.RefreshPricesAsync(command);

        Assert.IsType<AcceptedResult>(result);
        _mediator.Verify(m => m.SendAsync(command, It.IsAny<CancellationToken>()), Times.Once);
    }
}

public class QuotesOperationsControllerTests
{
    private readonly Mock<IMediator> _mediator = new();

    [Fact]
    public async Task RefreshQuotes_ReturnsAccepted()
    {
        var controller = new QuotesOperationsController(_mediator.Object);

        var result = await controller.RefreshQuotesAsync();

        Assert.IsType<AcceptedResult>(result);
        _mediator.Verify(m => m.SendAsync(
            It.IsAny<TriggerRefreshQuotesCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}

public class SnapshotsOperationsControllerTests
{
    private readonly Mock<IMediator> _mediator = new();

    [Fact]
    public async Task BackfillSnapshots_ReturnsAccepted()
    {
        var controller = new SnapshotsOperationsController(_mediator.Object);
        var command = new TriggerBackfillSnapshotsCommand(null, null, null);

        var result = await controller.BackfillSnapshotsAsync(command);

        Assert.IsType<AcceptedResult>(result);
        _mediator.Verify(m => m.SendAsync(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CalculateSnapshots_ReturnsAccepted()
    {
        var controller = new SnapshotsOperationsController(_mediator.Object);
        var command = new TriggerCalculateSnapshotsCommand(2024);

        var result = await controller.CalculateSnapshotsAsync(command);

        Assert.IsType<AcceptedResult>(result);
        _mediator.Verify(m => m.SendAsync(command, It.IsAny<CancellationToken>()), Times.Once);
    }
}

public class ValuationsOperationsControllerTests
{
    private readonly Mock<IMediator> _mediator = new();

    [Fact]
    public async Task BackfillValuations_ReturnsAccepted()
    {
        var controller = new ValuationsOperationsController(_mediator.Object);
        var command = new TriggerBackfillValuationsCommand(null, null, null);

        var result = await controller.BackfillValuationsAsync(command);

        Assert.IsType<AcceptedResult>(result);
        _mediator.Verify(m => m.SendAsync(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CalculateValuations_ReturnsAccepted()
    {
        var controller = new ValuationsOperationsController(_mediator.Object);
        var command = new TriggerCalculateValuationsCommand(new DateOnly(2024, 1, 15));

        var result = await controller.CalculateValuationsAsync(command);

        Assert.IsType<AcceptedResult>(result);
        _mediator.Verify(m => m.SendAsync(command, It.IsAny<CancellationToken>()), Times.Once);
    }
}
