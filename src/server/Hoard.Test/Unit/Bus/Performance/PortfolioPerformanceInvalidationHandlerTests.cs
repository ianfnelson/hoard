using Hoard.Bus.Performance;
using Hoard.Messages;
using Hoard.Messages.Performance;
using Hoard.Messages.Valuations;
using Moq;
using Rebus.Bus;

namespace Hoard.Test.Unit.Bus.Performance;

public class PortfolioPerformanceInvalidationHandlerTests
{
    private readonly Mock<IBus> _bus = new();
    private readonly PortfolioPerformanceInvalidationHandler _handler;

    public PortfolioPerformanceInvalidationHandlerTests()
    {
        _handler = new PortfolioPerformanceInvalidationHandler(_bus.Object);
    }

    [Fact]
    public async Task Handle_PositionPerformanceCalculated_DaytimeReactive_PublishesInvalidation()
    {
        await _handler.Handle(new PositionPerformanceCalculatedEvent(Guid.NewGuid(), 42, PipelineMode.DaytimeReactive));

        _bus.Verify(b => b.Publish(It.IsAny<PortfolioPerformancesInvalidatedEvent>(), null), Times.Once);
    }

    [Theory]
    [InlineData(PipelineMode.CloseOfDay)]
    [InlineData(PipelineMode.Backfill)]
    [InlineData(PipelineMode.Overnight)]
    public async Task Handle_PositionPerformanceCalculated_NonDaytime_DoesNotPublish(PipelineMode mode)
    {
        await _handler.Handle(new PositionPerformanceCalculatedEvent(Guid.NewGuid(), 42, mode));

        _bus.Verify(b => b.Publish(It.IsAny<PortfolioPerformancesInvalidatedEvent>(), null), Times.Never);
    }

    [Fact]
    public async Task Handle_PortfolioValuationChanged_DaytimeReactive_PublishesInvalidation()
    {
        await _handler.Handle(new PortfolioValuationChangedEvent(
            Guid.NewGuid(), PipelineMode.DaytimeReactive, 10, new DateOnly(2024, 6, 15)));

        _bus.Verify(b => b.Publish(It.IsAny<PortfolioPerformancesInvalidatedEvent>(), null), Times.Once);
    }

    [Theory]
    [InlineData(PipelineMode.CloseOfDay)]
    [InlineData(PipelineMode.Backfill)]
    [InlineData(PipelineMode.Overnight)]
    public async Task Handle_PortfolioValuationChanged_NonDaytime_DoesNotPublish(PipelineMode mode)
    {
        await _handler.Handle(new PortfolioValuationChangedEvent(
            Guid.NewGuid(), mode, 10, new DateOnly(2024, 6, 15)));

        _bus.Verify(b => b.Publish(It.IsAny<PortfolioPerformancesInvalidatedEvent>(), null), Times.Never);
    }
}
