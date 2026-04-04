using Hoard.Bus.Performance;
using Hoard.Core.Application;
using Hoard.Core.Application.Performance;
using Hoard.Core.Application.Portfolios;
using Hoard.Messages;
using Hoard.Messages.Performance;
using Microsoft.Extensions.Logging;
using Moq;
using Rebus.Bus;

namespace Hoard.Test.Unit.Bus.Performance;

public class CalculatePerformanceSagaTests
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly Mock<IBus> _bus = new();
    private readonly CalculatePerformanceSaga _saga;

    public CalculatePerformanceSagaTests()
    {
        _saga = new CalculatePerformanceSaga(_mediator.Object, _bus.Object, Mock.Of<ILogger<CalculatePerformanceSaga>>())
        {
            Data = new CpSagaData()
        };
    }

    [Fact]
    public async Task Handle_Start_WithInstruments_DispatchesPositionPerformance()
    {
        var runId = Guid.NewGuid();
        var instrumentIds = new List<int> { 1, 2 };

        _mediator.Setup(m => m.QueryAsync<GetInstrumentsForPerformanceQuery, IReadOnlyList<int>>(
                It.IsAny<GetInstrumentsForPerformanceQuery>(), default))
            .ReturnsAsync(instrumentIds);

        await _saga.Handle(new StartCalculatePerformanceSagaCommand(runId, null, PipelineMode.CloseOfDay));

        Assert.Equal(runId, _saga.Data.PerformanceRunId);
        Assert.Equal(new HashSet<int> { 1, 2 }, _saga.Data.PendingInstruments);

        _mediator.Verify(m => m.SendAsync(It.IsAny<DispatchCalculatePositionPerformanceCommand>(), default), Times.Once);
        _mediator.Verify(m => m.SendAsync(It.IsAny<DispatchCalculatePortfolioPerformanceCommand>(), default), Times.Never);
    }

    [Fact]
    public async Task Handle_Start_WithNoInstruments_SkipsToPortfolioPerformance()
    {
        var runId = Guid.NewGuid();
        var portfolioIds = new List<int> { 10, 20 };

        _mediator.Setup(m => m.QueryAsync<GetInstrumentsForPerformanceQuery, IReadOnlyList<int>>(
                It.IsAny<GetInstrumentsForPerformanceQuery>(), default))
            .ReturnsAsync(new List<int>());

        _mediator.Setup(m => m.QueryAsync<GetPortfolioIdsQuery, IReadOnlyList<int>>(
                It.IsAny<GetPortfolioIdsQuery>(), default))
            .ReturnsAsync(portfolioIds);

        await _saga.Handle(new StartCalculatePerformanceSagaCommand(runId, null, PipelineMode.CloseOfDay));

        Assert.Equal(new HashSet<int> { 10, 20 }, _saga.Data.PendingPortfolios);
        _mediator.Verify(m => m.SendAsync(It.IsAny<DispatchCalculatePortfolioPerformanceCommand>(), default), Times.Once);
        _bus.Verify(b => b.Publish(It.IsAny<PerformanceCalculatedEvent>(), null), Times.Never);
    }

    [Fact]
    public async Task Handle_Start_WithNoInstrumentsOrPortfolios_CompletesImmediately()
    {
        var runId = Guid.NewGuid();

        _mediator.Setup(m => m.QueryAsync<GetInstrumentsForPerformanceQuery, IReadOnlyList<int>>(
                It.IsAny<GetInstrumentsForPerformanceQuery>(), default))
            .ReturnsAsync(new List<int>());

        _mediator.Setup(m => m.QueryAsync<GetPortfolioIdsQuery, IReadOnlyList<int>>(
                It.IsAny<GetPortfolioIdsQuery>(), default))
            .ReturnsAsync(new List<int>());

        await _saga.Handle(new StartCalculatePerformanceSagaCommand(runId, null, PipelineMode.CloseOfDay));

        _bus.Verify(b => b.Publish(It.Is<PerformanceCalculatedEvent>(e =>
            e.PerformanceRunId == runId && e.PipelineMode == PipelineMode.CloseOfDay), null), Times.Once);
    }

    [Fact]
    public async Task Handle_LastPositionPerformanceCalculated_DispatchesPortfolioPerformance()
    {
        var runId = Guid.NewGuid();
        _saga.Data.PerformanceRunId = runId;
        _saga.Data.PendingInstruments = new HashSet<int> { 1 };

        var portfolioIds = new List<int> { 10 };
        _mediator.Setup(m => m.QueryAsync<GetPortfolioIdsQuery, IReadOnlyList<int>>(
                It.IsAny<GetPortfolioIdsQuery>(), default))
            .ReturnsAsync(portfolioIds);

        await _saga.Handle(new PositionPerformanceCalculatedEvent(runId, 1, PipelineMode.CloseOfDay));

        Assert.Empty(_saga.Data.PendingInstruments);
        Assert.Equal(new HashSet<int> { 10 }, _saga.Data.PendingPortfolios);
        _mediator.Verify(m => m.SendAsync(It.IsAny<DispatchCalculatePortfolioPerformanceCommand>(), default), Times.Once);
    }

    [Fact]
    public async Task Handle_PositionPerformanceCalculated_NotLast_DoesNotDispatchPortfolio()
    {
        var runId = Guid.NewGuid();
        _saga.Data.PerformanceRunId = runId;
        _saga.Data.PendingInstruments = new HashSet<int> { 1, 2 };

        await _saga.Handle(new PositionPerformanceCalculatedEvent(runId, 1, PipelineMode.CloseOfDay));

        Assert.Single(_saga.Data.PendingInstruments);
        _mediator.Verify(m => m.SendAsync(It.IsAny<DispatchCalculatePortfolioPerformanceCommand>(), default), Times.Never);
    }

    [Fact]
    public async Task Handle_LastPortfolioPerformanceCalculated_PublishesCompletionEvent()
    {
        var runId = Guid.NewGuid();
        _saga.Data.PerformanceRunId = runId;
        _saga.Data.PendingPortfolios = new HashSet<int> { 10 };

        await _saga.Handle(new PortfolioPerformanceCalculatedEvent(runId, 10, PipelineMode.CloseOfDay));

        Assert.Empty(_saga.Data.PendingPortfolios);
        _bus.Verify(b => b.Publish(It.Is<PerformanceCalculatedEvent>(e =>
            e.PerformanceRunId == runId && e.PipelineMode == PipelineMode.CloseOfDay), null), Times.Once);
    }

    [Fact]
    public async Task Handle_PortfolioPerformanceCalculated_NotLast_DoesNotComplete()
    {
        var runId = Guid.NewGuid();
        _saga.Data.PerformanceRunId = runId;
        _saga.Data.PendingPortfolios = new HashSet<int> { 10, 20 };

        await _saga.Handle(new PortfolioPerformanceCalculatedEvent(runId, 10, PipelineMode.CloseOfDay));

        Assert.Single(_saga.Data.PendingPortfolios);
        _bus.Verify(b => b.Publish(It.IsAny<PerformanceCalculatedEvent>(), null), Times.Never);
    }
}
