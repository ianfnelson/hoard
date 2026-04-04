using Hoard.Bus.Holdings;
using Hoard.Bus.Prices;
using Hoard.Bus.Quotes;
using Hoard.Bus.Analysis;
using Hoard.Core.Application;
using Hoard.Core.Application.Analysis;
using Hoard.Core.Application.Holdings;
using Hoard.Core.Application.Prices;
using Hoard.Core.Application.Quotes;
using Hoard.Messages;
using Hoard.Messages.Analysis;
using Hoard.Messages.Holdings;
using Hoard.Messages.Prices;
using Hoard.Messages.Quotes;
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
