using Hoard.Core.Application.Alerts;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Test.Shared;
using Moq;

namespace Hoard.Test.Unit.Core.Application.Alerts;

public class CloseOfDayAlertHandlerTests : IDisposable
{
    private readonly HoardContext _context;
    private readonly Mock<IAlertService> _alertService = new();
    private readonly CloseOfDayAlertHandler _handler;

    public CloseOfDayAlertHandlerTests()
    {
        _context = TestContextFactory.Create();
        _handler = new CloseOfDayAlertHandler(_context, _alertService.Object);
    }

    private static DateOnly TodayIfWeekday()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        while (today.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            today = today.AddDays(1);
        return today;
    }

    [Fact]
    public async Task HandleAsync_NotToday_DoesNotRaiseAlerts()
    {
        var yesterday = DateOnly.FromDateTime(DateTime.Now).AddDays(-1);

        await _handler.HandleAsync(new CloseOfDayAlertCommand(yesterday));

        _alertService.Verify(a => a.TryRaiseAsync(It.IsAny<AlertRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_Weekend_DoesNotRaiseAlerts()
    {
        var date = DateOnly.FromDateTime(DateTime.Now);
        while (date.DayOfWeek != DayOfWeek.Saturday) date = date.AddDays(1);

        await _handler.HandleAsync(new CloseOfDayAlertCommand(date));

        _alertService.Verify(a => a.TryRaiseAsync(It.IsAny<AlertRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_RaisesAlertPerPortfolioPerformance()
    {
        var today = TodayIfWeekday();
        // This test can only run if today is a weekday
        if (DateOnly.FromDateTime(DateTime.Now) != today)
            return;

        var portfolio1 = TestData.Portfolio(id: 1, name: "ISA");
        var portfolio2 = TestData.Portfolio(id: 2, name: "SIPP");
        _context.Portfolios.AddRange(portfolio1, portfolio2);
        await _context.SaveChangesAsync();

        _context.PortfolioPerformances.AddRange(
            new PortfolioPerformance { PortfolioId = 1, Value = 10000m, ValueChange1D = 100m, Return1D = 1.0m, Return1Y = 10.0m },
            new PortfolioPerformance { PortfolioId = 2, Value = 50000m, ValueChange1D = -200m, Return1D = -0.4m, Return1Y = 8.5m }
        );
        await _context.SaveChangesAsync();

        await _handler.HandleAsync(new CloseOfDayAlertCommand(today));

        _alertService.Verify(a => a.TryRaiseAsync(It.IsAny<AlertRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task HandleAsync_AlertRequestHasCorrectType()
    {
        var today = TodayIfWeekday();
        if (DateOnly.FromDateTime(DateTime.Now) != today)
            return;

        var portfolio = TestData.Portfolio();
        _context.Portfolios.Add(portfolio);
        await _context.SaveChangesAsync();

        _context.PortfolioPerformances.Add(
            new PortfolioPerformance { PortfolioId = portfolio.Id, Value = 10000m });
        await _context.SaveChangesAsync();

        await _handler.HandleAsync(new CloseOfDayAlertCommand(today));

        _alertService.Verify(a => a.TryRaiseAsync(It.Is<AlertRequest>(r =>
            r.AlertTypeId == AlertType.CloseOfDayPortfolioValuation &&
            r.PortfolioId == portfolio.Id &&
            r.TriggeredDate == today), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_NoPerformances_RaisesNoAlerts()
    {
        var today = TodayIfWeekday();
        if (DateOnly.FromDateTime(DateTime.Now) != today)
            return;

        await _handler.HandleAsync(new CloseOfDayAlertCommand(today));

        _alertService.Verify(a => a.TryRaiseAsync(It.IsAny<AlertRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
