using Hoard.Bus.Valuations;
using Hoard.Core.Application;
using Hoard.Core.Application.Portfolios;
using Hoard.Core.Application.Valuations;
using Hoard.Messages;
using Hoard.Messages.Valuations;
using Moq;
using Rebus.Bus;

namespace Hoard.Test.Unit.Bus.Valuations;

public class PortfolioValuationsRecalculationSagaTests
{
    private readonly Mock<IBus> _bus = new();
    private readonly Mock<IMediator> _mediator = new();
    private readonly PortfolioValuationsRecalculationSaga _saga;

    public PortfolioValuationsRecalculationSagaTests()
    {
        _saga = new PortfolioValuationsRecalculationSaga(_bus.Object, _mediator.Object)
        {
            Data = new PvrSagaData()
        };
    }

    [Fact]
    public async Task Handle_Invalidated_WhenNotScheduled_SchedulesTimeoutAndStoresDate()
    {
        var asOfDate = new DateOnly(2024, 6, 15);

        await _saga.Handle(new PortfolioValuationsInvalidatedEvent(PipelineMode.DaytimeReactive, asOfDate));

        Assert.True(_saga.Data.IsScheduled);
        Assert.Equal(asOfDate, _saga.Data.AsOfDate);
        _bus.Verify(b => b.DeferLocal(
            It.Is<TimeSpan>(ts => ts == TimeSpan.FromSeconds(3)),
            It.Is<RecalculatePortfolioValuationsTimeout>(t =>
                t.PipelineMode == PipelineMode.DaytimeReactive && t.AsOfDate == asOfDate),
            null), Times.Once);
    }

    [Fact]
    public async Task Handle_Invalidated_WhenAlreadyScheduled_DoesNotScheduleAgain()
    {
        _saga.Data.IsScheduled = true;

        await _saga.Handle(new PortfolioValuationsInvalidatedEvent(PipelineMode.DaytimeReactive, new DateOnly(2024, 6, 15)));

        _bus.Verify(b => b.DeferLocal(
            It.IsAny<TimeSpan>(),
            It.IsAny<RecalculatePortfolioValuationsTimeout>(),
            null), Times.Never);
    }

    [Fact]
    public async Task Handle_Timeout_DispatchesPortfolioValuationsAndCompletes()
    {
        var asOfDate = new DateOnly(2024, 6, 15);
        var portfolioIds = new List<int> { 1, 2 };

        _mediator.Setup(m => m.QueryAsync<GetPortfolioIdsQuery, IReadOnlyList<int>>(
                It.IsAny<GetPortfolioIdsQuery>(), default))
            .ReturnsAsync(portfolioIds);

        _saga.Data.IsScheduled = true;

        await _saga.Handle(new RecalculatePortfolioValuationsTimeout(PipelineMode.DaytimeReactive, asOfDate));

        Assert.False(_saga.Data.IsScheduled);
        _mediator.Verify(m => m.SendAsync(
            It.Is<DispatchCalculatePortfolioValuationCommand>(c =>
                c.PipelineMode == PipelineMode.DaytimeReactive &&
                c.AsOfDate == asOfDate),
            default), Times.Once);
    }
}
