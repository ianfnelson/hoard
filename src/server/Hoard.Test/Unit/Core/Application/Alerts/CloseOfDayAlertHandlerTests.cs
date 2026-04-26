using Hoard.Core;
using Hoard.Core.Application.Alerts;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Test.Shared;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace Hoard.Test.Unit.Core.Application.Alerts;

public class CloseOfDayAlertHandlerTests : IDisposable
{
    private static readonly DateOnly Wednesday = new(2026, 4, 8);
    private static readonly DateOnly Saturday = new(2026, 4, 11);

    private readonly HoardContext _context;
    private readonly Mock<IPortfolioAlertEvaluator> _evaluator = new();
    private readonly Mock<ICloseOfDayPortfolioAlertEvaluator> _codEvaluator = new();
    private readonly FakeTimeProvider _timeProvider = new();
    private readonly CloseOfDayAlertHandler _handler;

    public CloseOfDayAlertHandlerTests()
    {
        _context = TestContextFactory.Create();
        _handler = new CloseOfDayAlertHandler(
            _context, [_evaluator.Object], [_codEvaluator.Object], _timeProvider);
    }

    private void SetUkDate(DateOnly date)
    {
        var ukNoon = new DateTime(date.Year, date.Month, date.Day, 12, 0, 0, DateTimeKind.Unspecified);
        var utc = TimeZoneInfo.ConvertTimeToUtc(ukNoon, DateOnlyHelper.UkTimeZone);
        _timeProvider.SetUtcNow(new DateTimeOffset(utc, TimeSpan.Zero));
    }

    [Fact]
    public async Task HandleAsync_NotToday_DoesNotInvokeEvaluators()
    {
        SetUkDate(Wednesday);

        await _handler.HandleAsync(new CloseOfDayAlertCommand(Wednesday.AddDays(-1)));

        _evaluator.Verify(e => e.EvaluateAsync(
            It.IsAny<PortfolioPerformance>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _codEvaluator.Verify(e => e.EvaluateAsync(
            It.IsAny<PortfolioPerformance>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_Weekend_DoesNotInvokeEvaluators()
    {
        SetUkDate(Saturday);

        await _handler.HandleAsync(new CloseOfDayAlertCommand(Saturday));

        _evaluator.Verify(e => e.EvaluateAsync(
            It.IsAny<PortfolioPerformance>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _codEvaluator.Verify(e => e.EvaluateAsync(
            It.IsAny<PortfolioPerformance>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_NoPerformances_DoesNotInvokeEvaluators()
    {
        SetUkDate(Wednesday);

        await _handler.HandleAsync(new CloseOfDayAlertCommand(Wednesday));

        _evaluator.Verify(e => e.EvaluateAsync(
            It.IsAny<PortfolioPerformance>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _codEvaluator.Verify(e => e.EvaluateAsync(
            It.IsAny<PortfolioPerformance>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_InvokesEachEvaluatorPerPortfolioPerformance()
    {
        SetUkDate(Wednesday);

        var second = new Mock<IPortfolioAlertEvaluator>();
        var handler = new CloseOfDayAlertHandler(
            _context, [_evaluator.Object, second.Object], [_codEvaluator.Object], _timeProvider);

        _context.Portfolios.AddRange(
            TestData.Portfolio(id: 1, name: "ISA"),
            TestData.Portfolio(id: 2, name: "SIPP"));
        await _context.SaveChangesAsync();

        _context.PortfolioPerformances.AddRange(
            new PortfolioPerformance { PortfolioId = 1, Value = 10000m },
            new PortfolioPerformance { PortfolioId = 2, Value = 50000m });
        await _context.SaveChangesAsync();

        await handler.HandleAsync(new CloseOfDayAlertCommand(Wednesday));

        _evaluator.Verify(e => e.EvaluateAsync(
            It.Is<PortfolioPerformance>(p => p.PortfolioId == 1), Wednesday, It.IsAny<CancellationToken>()),
            Times.Once);
        _evaluator.Verify(e => e.EvaluateAsync(
            It.Is<PortfolioPerformance>(p => p.PortfolioId == 2), Wednesday, It.IsAny<CancellationToken>()),
            Times.Once);
        second.Verify(e => e.EvaluateAsync(
            It.IsAny<PortfolioPerformance>(), Wednesday, It.IsAny<CancellationToken>()),
            Times.Exactly(2));
        _codEvaluator.Verify(e => e.EvaluateAsync(
            It.IsAny<PortfolioPerformance>(), Wednesday, It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task HandleAsync_PassesPortfolioWithIncludedNavigation()
    {
        SetUkDate(Wednesday);

        _context.Portfolios.Add(TestData.Portfolio(id: 1, name: "ISA"));
        await _context.SaveChangesAsync();

        _context.PortfolioPerformances.Add(
            new PortfolioPerformance { PortfolioId = 1, Value = 10000m });
        await _context.SaveChangesAsync();

        await _handler.HandleAsync(new CloseOfDayAlertCommand(Wednesday));

        _evaluator.Verify(e => e.EvaluateAsync(
            It.Is<PortfolioPerformance>(p => p.Portfolio != null && p.Portfolio.Name == "ISA"),
            Wednesday,
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
