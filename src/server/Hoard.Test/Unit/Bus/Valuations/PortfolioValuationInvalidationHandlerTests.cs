using Hoard.Bus.Valuations;
using Hoard.Messages;
using Hoard.Messages.Valuations;
using Moq;
using Rebus.Bus;

namespace Hoard.Test.Unit.Bus.Valuations;

public class PortfolioValuationInvalidationHandlerTests
{
    private readonly Mock<IBus> _bus = new();
    private readonly PortfolioValuationInvalidationHandler _handler;

    public PortfolioValuationInvalidationHandlerTests()
    {
        _handler = new PortfolioValuationInvalidationHandler(_bus.Object);
    }

    [Fact]
    public async Task Handle_DaytimeReactive_PublishesInvalidation()
    {
        var asOfDate = new DateOnly(2024, 6, 15);

        await _handler.Handle(new HoldingValuationsChangedEvent(
            Guid.NewGuid(), PipelineMode.DaytimeReactive, 42, asOfDate));

        _bus.Verify(b => b.Publish(
            It.Is<PortfolioValuationsInvalidatedEvent>(e =>
                e.PipelineMode == PipelineMode.DaytimeReactive &&
                e.AsOfDate == asOfDate),
            null), Times.Once);
    }

    [Theory]
    [InlineData(PipelineMode.CloseOfDay)]
    [InlineData(PipelineMode.Backfill)]
    [InlineData(PipelineMode.Overnight)]
    public async Task Handle_NonDaytime_DoesNotPublish(PipelineMode mode)
    {
        await _handler.Handle(new HoldingValuationsChangedEvent(
            Guid.NewGuid(), mode, 42, new DateOnly(2024, 6, 15)));

        _bus.Verify(b => b.Publish(It.IsAny<PortfolioValuationsInvalidatedEvent>(), null), Times.Never);
    }
}
