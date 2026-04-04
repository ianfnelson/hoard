using Hoard.Bus.Holdings;
using Hoard.Bus.News;
using Hoard.Bus.Performance;
using Hoard.Bus.Positions;
using Hoard.Bus.Prices;
using Hoard.Bus.Quotes;
using Hoard.Bus.Snapshots;
using Hoard.Bus.Valuations;
using Hoard.Bus.Analysis;
using Hoard.Core.Application;
using Hoard.Core.Application.Analysis;
using Hoard.Core.Application.Holdings;
using Hoard.Core.Application.News;
using Hoard.Core.Application.Performance;
using Hoard.Core.Application.Positions;
using Hoard.Core.Application.Prices;
using Hoard.Core.Application.Quotes;
using Hoard.Core.Application.Snapshots;
using Hoard.Core.Application.Valuations;
using Hoard.Messages;
using Hoard.Messages.Analysis;
using Hoard.Messages.Holdings;
using Hoard.Messages.News;
using Hoard.Messages.Performance;
using Hoard.Messages.Positions;
using Hoard.Messages.Prices;
using Hoard.Messages.Quotes;
using Hoard.Messages.Snapshots;
using Hoard.Messages.Valuations;
using Moq;

namespace Hoard.Test.Unit.Bus.CommandHandlers;

public class RefreshPricesCommandHandlerTests
{
    [Fact]
    public async Task Handle_TranslatesBusCommandToAppCommand()
    {
        var mediator = new Mock<IMediator>();
        var handler = new RefreshPricesCommandHandler(mediator.Object);
        var runId = Guid.NewGuid();

        await handler.Handle(new RefreshPricesBatchBusCommand(
            runId, PipelineMode.CloseOfDay, 42, new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 31)));

        mediator.Verify(m => m.SendAsync(
            It.Is<ProcessRefreshPricesBatchCommand>(c =>
                c.PricesRunId == runId &&
                c.PipelineMode == PipelineMode.CloseOfDay &&
                c.InstrumentId == 42 &&
                c.StartDate == new DateOnly(2024, 1, 1) &&
                c.EndDate == new DateOnly(2024, 1, 31)),
            default), Times.Once);
    }
}

public class CalculateHoldingsCommandHandlerTests
{
    [Fact]
    public async Task Handle_TranslatesBusCommandToAppCommand()
    {
        var mediator = new Mock<IMediator>();
        var handler = new CalculateHoldingsCommandHandler(mediator.Object);
        var runId = Guid.NewGuid();
        var asOfDate = new DateOnly(2024, 6, 15);

        await handler.Handle(new CalculateHoldingsBusCommand(runId, PipelineMode.CloseOfDay, asOfDate));

        mediator.Verify(m => m.SendAsync(
            It.Is<ProcessCalculateHoldingsCommand>(c =>
                c.HoldingsRunId == runId &&
                c.PipelineMode == PipelineMode.CloseOfDay &&
                c.AsOfDate == asOfDate),
            default), Times.Once);
    }
}

public class RefreshQuotesCommandHandlerTests
{
    [Fact]
    public async Task Handle_TranslatesBusCommandToAppCommand()
    {
        var mediator = new Mock<IMediator>();
        var handler = new RefreshQuotesCommandHandler(mediator.Object);
        var runId = Guid.NewGuid();
        var instrumentIds = new List<int> { 42, 43 };

        await handler.Handle(new RefreshQuotesBatchBusCommand(runId, 0, instrumentIds));

        mediator.Verify(m => m.SendAsync(
            It.Is<ProcessRefreshQuotesBatchCommand>(c =>
                c.QuotesRunId == runId &&
                c.BatchIndex == 0 &&
                c.InstrumentIds == instrumentIds),
            default), Times.Once);
    }
}

public class AnalyseInstrumentsCommandHandlerTests
{
    [Fact]
    public async Task Handle_TranslatesBusCommandToAppCommand()
    {
        var mediator = new Mock<IMediator>();
        var handler = new AnalyseInstrumentsCommandHandler(mediator.Object);
        var runId = Guid.NewGuid();
        var asOfDate = new DateOnly(2024, 6, 15);

        await handler.Handle(new AnalyseInstrumentBatchBusCommand(runId, PipelineMode.CloseOfDay, 42, asOfDate));

        mediator.Verify(m => m.SendAsync(
            It.Is<ProcessAnalyseInstrumentCommand>(c =>
                c.AnalysisRunId == runId &&
                c.PipelineMode == PipelineMode.CloseOfDay &&
                c.InstrumentId == 42 &&
                c.AsOfDate == asOfDate),
            default), Times.Once);
    }
}

public class CalculatePerformanceCommandHandlerTests
{
    [Fact]
    public async Task Handle_PositionPerformance_TranslatesBusCommandToAppCommand()
    {
        var mediator = new Mock<IMediator>();
        var handler = new CalculatePerformanceCommandHandler(mediator.Object);
        var runId = Guid.NewGuid();

        await handler.Handle(new CalculatePositionPerformanceBusCommand(runId, 42, PipelineMode.CloseOfDay));

        mediator.Verify(m => m.SendAsync(
            It.Is<ProcessCalculatePositionPerformanceCommand>(c =>
                c.PerformanceRunId == runId &&
                c.InstrumentId == 42 &&
                c.PipelineMode == PipelineMode.CloseOfDay),
            default), Times.Once);
    }

    [Fact]
    public async Task Handle_PortfolioPerformance_TranslatesBusCommandToAppCommand()
    {
        var mediator = new Mock<IMediator>();
        var handler = new CalculatePerformanceCommandHandler(mediator.Object);
        var runId = Guid.NewGuid();

        await handler.Handle(new CalculatePortfolioPerformanceBusCommand(runId, 10, PipelineMode.CloseOfDay));

        mediator.Verify(m => m.SendAsync(
            It.Is<ProcessCalculatePortfolioPerformanceCommand>(c =>
                c.PerformanceRunId == runId &&
                c.PortfolioId == 10 &&
                c.PipelineMode == PipelineMode.CloseOfDay),
            default), Times.Once);
    }
}

public class CalculatePositionsCommandHandlerTests
{
    [Fact]
    public async Task Handle_TranslatesBusCommandToAppCommand()
    {
        var mediator = new Mock<IMediator>();
        var handler = new CalculatePositionsCommandHandler(mediator.Object);
        var runId = Guid.NewGuid();

        await handler.Handle(new CalculatePositionsBusCommand(runId, PipelineMode.CloseOfDay));

        mediator.Verify(m => m.SendAsync(
            It.Is<ProcessCalculatePositionsCommand>(c =>
                c.PositionsRunId == runId &&
                c.PipelineMode == PipelineMode.CloseOfDay),
            default), Times.Once);
    }
}

public class CalculateSnapshotCommandHandlerTests
{
    [Fact]
    public async Task Handle_TranslatesBusCommandToAppCommand()
    {
        var mediator = new Mock<IMediator>();
        var handler = new CalculateSnapshotCommandHandler(mediator.Object);
        var runId = Guid.NewGuid();

        await handler.Handle(new CalculateSnapshotBusCommand(runId, PipelineMode.CloseOfDay, 5, 2024));

        mediator.Verify(m => m.SendAsync(
            It.Is<ProcessCalculateSnapshotCommand>(c =>
                c.SnapshotsRunId == runId &&
                c.PipelineMode == PipelineMode.CloseOfDay &&
                c.PortfolioId == 5 &&
                c.Year == 2024),
            default), Times.Once);
    }
}

public class CalculateValuationsCommandHandlerTests
{
    [Fact]
    public async Task Handle_HoldingValuations_TranslatesBusCommandToAppCommand()
    {
        var mediator = new Mock<IMediator>();
        var handler = new CalculateValuationsCommandHandler(mediator.Object);
        var runId = Guid.NewGuid();
        var asOfDate = new DateOnly(2024, 6, 15);

        await handler.Handle(new CalculateHoldingValuationsBusCommand(runId, PipelineMode.CloseOfDay, 42, asOfDate));

        mediator.Verify(m => m.SendAsync(
            It.Is<ProcessCalculateHoldingValuationsCommand>(c =>
                c.ValuationsRunId == runId &&
                c.PipelineMode == PipelineMode.CloseOfDay &&
                c.InstrumentId == 42 &&
                c.AsOfDate == asOfDate),
            default), Times.Once);
    }

    [Fact]
    public async Task Handle_PortfolioValuation_TranslatesBusCommandToAppCommand()
    {
        var mediator = new Mock<IMediator>();
        var handler = new CalculateValuationsCommandHandler(mediator.Object);
        var runId = Guid.NewGuid();
        var asOfDate = new DateOnly(2024, 6, 15);

        await handler.Handle(new CalculatePortfolioValuationBusCommand(runId, PipelineMode.CloseOfDay, 10, asOfDate));

        mediator.Verify(m => m.SendAsync(
            It.Is<ProcessCalculatePortfolioValuationCommand>(c =>
                c.ValuationsRunId == runId &&
                c.PipelineMode == PipelineMode.CloseOfDay &&
                c.PortfolioId == 10 &&
                c.AsOfDate == asOfDate),
            default), Times.Once);
    }
}

public class RefreshNewsCommandHandlerTests
{
    [Fact]
    public async Task Handle_TranslatesBusCommandToAppCommand()
    {
        var mediator = new Mock<IMediator>();
        var handler = new RefreshNewsCommandHandler(mediator.Object);
        var runId = Guid.NewGuid();

        await handler.Handle(new RefreshNewsBatchBusCommand(runId, PipelineMode.CloseOfDay, 42));

        mediator.Verify(m => m.SendAsync(
            It.Is<ProcessRefreshNewsBatchCommand>(c =>
                c.NewsRunId == runId &&
                c.PipelineMode == PipelineMode.CloseOfDay &&
                c.InstrumentId == 42),
            default), Times.Once);
    }
}
