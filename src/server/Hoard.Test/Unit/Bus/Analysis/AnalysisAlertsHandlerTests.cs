using Hoard.Bus.Analysis;
using Hoard.Core.Application;
using Hoard.Core.Application.Alerts;
using Hoard.Messages;
using Hoard.Messages.Analysis;
using Moq;

namespace Hoard.Test.Unit.Bus.Analysis;

public class AnalysisAlertsHandlerTests
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly AnalysisAlertsHandler _handler;

    public AnalysisAlertsHandlerTests()
    {
        _handler = new AnalysisAlertsHandler(_mediator.Object);
    }

    [Theory]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    [InlineData(true, true, true)]
    public async Task Handle_WithAlertCondition_DispatchesAlertCommand(bool isHigh, bool isLow, bool isAbnormal)
    {
        var asOfDate = new DateOnly(2024, 6, 15);

        await _handler.Handle(new InstrumentAnalysedEvent(
            Guid.NewGuid(), PipelineMode.CloseOfDay, 42, asOfDate, isHigh, isLow, isAbnormal));

        _mediator.Verify(m => m.SendAsync(
            It.Is<InstrumentAnalysisAlertCommand>(c =>
                c.InstrumentId == 42 &&
                c.AsOfDate == asOfDate &&
                c.IsNewFiftyTwoWeekHigh == isHigh &&
                c.IsNewFiftyTwoWeekLow == isLow &&
                c.IsAbnormalVolume == isAbnormal),
            default), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNoAlertConditions_DoesNotDispatch()
    {
        await _handler.Handle(new InstrumentAnalysedEvent(
            Guid.NewGuid(), PipelineMode.CloseOfDay, 42, new DateOnly(2024, 6, 15),
            false, false, false));

        _mediator.Verify(m => m.SendAsync(It.IsAny<InstrumentAnalysisAlertCommand>(), default), Times.Never);
    }
}
