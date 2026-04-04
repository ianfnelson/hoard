using Hoard.Bus.Performance;
using Hoard.Core.Application;
using Hoard.Core.Application.Performance;
using Hoard.Core.Application.Portfolios;
using Hoard.Messages;
using Hoard.Messages.Performance;
using Moq;
using Rebus.Bus;

namespace Hoard.Test.Unit.Bus.Performance;

public class PortfolioPerformanceRecalculationSagaTests
{
    private readonly Mock<IBus> _bus = new();
    private readonly Mock<IMediator> _mediator = new();
    private readonly PortfolioPerformanceRecalculationSaga _saga;

    public PortfolioPerformanceRecalculationSagaTests()
    {
        _saga = new PortfolioPerformanceRecalculationSaga(_bus.Object, _mediator.Object)
        {
            Data = new PprSagaData()
        };
    }

    [Fact]
    public async Task Handle_Invalidated_WhenNotScheduled_SchedulesTimeout()
    {
        await _saga.Handle(new PortfolioPerformancesInvalidatedEvent(PipelineMode.DaytimeReactive));

        Assert.True(_saga.Data.IsScheduled);
        _bus.Verify(b => b.DeferLocal(
            It.Is<TimeSpan>(ts => ts == TimeSpan.FromSeconds(5)),
            It.IsAny<RecalculatePortfolioPerformancesTimeout>(),
            null), Times.Once);
    }

    [Fact]
    public async Task Handle_Invalidated_WhenAlreadyScheduled_DoesNotScheduleAgain()
    {
        _saga.Data.IsScheduled = true;

        await _saga.Handle(new PortfolioPerformancesInvalidatedEvent(PipelineMode.DaytimeReactive));

        _bus.Verify(b => b.DeferLocal(
            It.IsAny<TimeSpan>(),
            It.IsAny<RecalculatePortfolioPerformancesTimeout>(),
            null), Times.Never);
    }

    [Fact]
    public async Task Handle_Timeout_DispatchesPortfolioPerformanceAndCompletes()
    {
        var portfolioIds = new List<int> { 1, 2, 3 };

        _mediator.Setup(m => m.QueryAsync<GetPortfolioIdsQuery, IReadOnlyList<int>>(
                It.IsAny<GetPortfolioIdsQuery>(), default))
            .ReturnsAsync(portfolioIds);

        _saga.Data.IsScheduled = true;

        await _saga.Handle(new RecalculatePortfolioPerformancesTimeout(PipelineMode.DaytimeReactive));

        Assert.False(_saga.Data.IsScheduled);
        _mediator.Verify(m => m.SendAsync(
            It.Is<DispatchCalculatePortfolioPerformanceCommand>(c =>
                c.PipelineMode == PipelineMode.DaytimeReactive),
            default), Times.Once);
    }
}
