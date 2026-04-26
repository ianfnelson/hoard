using Hoard.Bus.Performance;
using Hoard.Core.Application;
using Hoard.Core.Application.Alerts;
using Hoard.Messages;
using Hoard.Messages.Performance;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace Hoard.Test.Unit.Bus.Performance;

public class PortfolioPerformanceAlertsHandlerTests
{
    private readonly Mock<IMediator> _mediator = new();

    // Monday 28 April 2026 at 13:00 UTC (weekday in UK timezone)
    private static readonly DateTimeOffset Weekday = new(2026, 4, 28, 13, 0, 0, TimeSpan.Zero);
    // Sunday 26 April 2026 at 13:00 UTC
    private static readonly DateTimeOffset Weekend = new(2026, 4, 26, 13, 0, 0, TimeSpan.Zero);

    private PortfolioPerformanceAlertsHandler CreateHandler(DateTimeOffset utcNow)
    {
        var timeProvider = new FakeTimeProvider(utcNow);
        return new PortfolioPerformanceAlertsHandler(_mediator.Object, timeProvider);
    }

    [Fact]
    public async Task Handle_DaytimeReactive_DispatchesPortfolioPerformanceAlertCommand()
    {
        var handler = CreateHandler(Weekday);
        var message = new PortfolioPerformanceCalculatedEvent(Guid.NewGuid(), 42, PipelineMode.DaytimeReactive);

        await handler.Handle(message);

        _mediator.Verify(m => m.SendAsync(
            It.Is<PortfolioPerformanceAlertCommand>(c =>
                c.PortfolioId == 42 &&
                c.AsOfDate == new DateOnly(2026, 4, 28)),
            default), Times.Once);
    }

    [Fact]
    public async Task Handle_CloseOfDay_DoesNotDispatchCommand()
    {
        var handler = CreateHandler(Weekday);
        var message = new PortfolioPerformanceCalculatedEvent(Guid.NewGuid(), 42, PipelineMode.CloseOfDay);

        await handler.Handle(message);

        _mediator.Verify(m => m.SendAsync(It.IsAny<PortfolioPerformanceAlertCommand>(), default),
            Times.Never);
    }

    [Fact]
    public async Task Handle_DaytimeReactiveOnWeekend_DoesNotDispatchCommand()
    {
        var handler = CreateHandler(Weekend);
        var message = new PortfolioPerformanceCalculatedEvent(Guid.NewGuid(), 42, PipelineMode.DaytimeReactive);

        await handler.Handle(message);

        _mediator.Verify(m => m.SendAsync(It.IsAny<PortfolioPerformanceAlertCommand>(), default),
            Times.Never);
    }
}
