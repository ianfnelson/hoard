using Hoard.Core.Application.Alerts;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Test.Shared;
using Moq;

namespace Hoard.Test.Unit.Core.Application.Alerts;

public class PortfolioPerformanceAlertHandlerTests : IDisposable
{
    private static readonly DateOnly AsOfDate = new(2026, 4, 28);

    private readonly HoardContext _context;
    private readonly Mock<IPortfolioAlertEvaluator> _evaluator = new();
    private readonly PortfolioPerformanceAlertHandler _handler;

    public PortfolioPerformanceAlertHandlerTests()
    {
        _context = TestContextFactory.Create();
        _handler = new PortfolioPerformanceAlertHandler(_context, [_evaluator.Object]);
    }

    [Fact]
    public async Task HandleAsync_NoPortfolioPerformance_DoesNotInvokeEvaluators()
    {
        await _handler.HandleAsync(new PortfolioPerformanceAlertCommand(42, AsOfDate));

        _evaluator.Verify(e => e.EvaluateAsync(
            It.IsAny<PortfolioPerformance>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_InvokesEachIntradayEvaluatorWithIncludedPortfolio()
    {
        var second = new Mock<IPortfolioAlertEvaluator>();
        var handler = new PortfolioPerformanceAlertHandler(
            _context, [_evaluator.Object, second.Object]);

        _context.Portfolios.Add(TestData.Portfolio(id: 1, name: "ISA"));
        await _context.SaveChangesAsync();
        _context.PortfolioPerformances.Add(
            new PortfolioPerformance { PortfolioId = 1, Value = 10000m });
        await _context.SaveChangesAsync();

        await handler.HandleAsync(new PortfolioPerformanceAlertCommand(1, AsOfDate));

        _evaluator.Verify(e => e.EvaluateAsync(
            It.Is<PortfolioPerformance>(p => p.PortfolioId == 1 && p.Portfolio.Name == "ISA"),
            AsOfDate, It.IsAny<CancellationToken>()),
            Times.Once);
        second.Verify(e => e.EvaluateAsync(
            It.Is<PortfolioPerformance>(p => p.PortfolioId == 1),
            AsOfDate, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
