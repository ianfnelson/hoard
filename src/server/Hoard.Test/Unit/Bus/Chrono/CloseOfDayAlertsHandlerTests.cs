using Hoard.Bus.Chrono;
using Hoard.Core.Application;
using Hoard.Core.Application.Alerts;
using Hoard.Messages;
using Hoard.Messages.Chrono;
using Moq;

namespace Hoard.Test.Unit.Bus.Chrono;

public class CloseOfDayAlertsHandlerTests
{
    [Fact]
    public async Task Handle_DispatchesCloseOfDayAlertCommand()
    {
        var mediator = new Mock<IMediator>();
        var handler = new CloseOfDayAlertsHandler(mediator.Object);
        var asOfDate = new DateOnly(2024, 6, 15);

        await handler.Handle(new CloseOfDayCompletedEvent(Guid.NewGuid(), asOfDate, PipelineMode.CloseOfDay));

        mediator.Verify(m => m.SendAsync(
            It.Is<CloseOfDayAlertCommand>(c => c.AsOfDate == asOfDate),
            default), Times.Once);
    }
}
