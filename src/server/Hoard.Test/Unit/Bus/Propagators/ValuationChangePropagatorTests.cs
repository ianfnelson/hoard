using Hoard.Bus.Valuations;
using Hoard.Core;
using Hoard.Core.Application;
using Hoard.Core.Application.Performance;
using Hoard.Core.Application.Snapshots;
using Hoard.Messages;
using Hoard.Messages.Valuations;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace Hoard.Test.Unit.Bus.Propagators;

public class ValuationChangePropagatorTests
{
    private static readonly DateOnly Today = new(2024, 6, 15);
    private static readonly DateOnly Yesterday = Today.AddDays(-1);

    private readonly Mock<IMediator> _mediator = new();
    private readonly FakeTimeProvider _timeProvider = new();
    private readonly ValuationChangePropagator _propagator;

    public ValuationChangePropagatorTests()
    {
        var ukNoon = new DateTime(Today.Year, Today.Month, Today.Day, 12, 0, 0, DateTimeKind.Unspecified);
        var utc = TimeZoneInfo.ConvertTimeToUtc(ukNoon, DateOnlyHelper.UkTimeZone);
        _timeProvider.SetUtcNow(new DateTimeOffset(utc, TimeSpan.Zero));

        _propagator = new ValuationChangePropagator(_mediator.Object, _timeProvider);
    }

    [Fact]
    public async Task Handle_HoldingValuationsChanged_DaytimeReactive_Today_DispatchesPositionPerformance()
    {
        await _propagator.Handle(new HoldingValuationsChangedEvent(
            Guid.NewGuid(), PipelineMode.DaytimeReactive, 1, Today));

        _mediator.Verify(m => m.SendAsync(
            It.Is<ProcessCalculatePositionPerformanceCommand>(c =>
                c.InstrumentId == 1 && c.PipelineMode == PipelineMode.DaytimeReactive),
            default), Times.Once);
    }

    [Fact]
    public async Task Handle_HoldingValuationsChanged_DaytimeReactive_NotToday_DoesNotDispatch()
    {
        await _propagator.Handle(new HoldingValuationsChangedEvent(
            Guid.NewGuid(), PipelineMode.DaytimeReactive, 1, Yesterday));

        _mediator.Verify(m => m.SendAsync(It.IsAny<ProcessCalculatePositionPerformanceCommand>(), default), Times.Never);
    }

    [Fact]
    public async Task Handle_HoldingValuationsChanged_CloseOfDay_DoesNotDispatch()
    {
        await _propagator.Handle(new HoldingValuationsChangedEvent(
            Guid.NewGuid(), PipelineMode.CloseOfDay, 1, Today));

        _mediator.Verify(m => m.SendAsync(It.IsAny<ProcessCalculatePositionPerformanceCommand>(), default), Times.Never);
    }

    [Fact]
    public async Task Handle_PortfolioValuationChanged_DaytimeReactive_Today_DispatchesSnapshot()
    {
        await _propagator.Handle(new PortfolioValuationChangedEvent(
            Guid.NewGuid(), PipelineMode.DaytimeReactive, 5, Today));

        _mediator.Verify(m => m.SendAsync(
            It.Is<ProcessCalculateSnapshotCommand>(c =>
                c.PortfolioId == 5 && c.Year == Today.Year),
            default), Times.Once);
    }

    [Fact]
    public async Task Handle_PortfolioValuationChanged_DaytimeReactive_NotToday_DoesNotDispatch()
    {
        await _propagator.Handle(new PortfolioValuationChangedEvent(
            Guid.NewGuid(), PipelineMode.DaytimeReactive, 5, Yesterday));

        _mediator.Verify(m => m.SendAsync(It.IsAny<ProcessCalculateSnapshotCommand>(), default), Times.Never);
    }

    [Fact]
    public async Task Handle_PortfolioValuationChanged_CloseOfDay_DoesNotDispatch()
    {
        await _propagator.Handle(new PortfolioValuationChangedEvent(
            Guid.NewGuid(), PipelineMode.CloseOfDay, 5, Today));

        _mediator.Verify(m => m.SendAsync(It.IsAny<ProcessCalculateSnapshotCommand>(), default), Times.Never);
    }
}
