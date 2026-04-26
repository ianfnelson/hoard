using Hoard.Core.Application.Alerts;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Test.Shared;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Hoard.Test.Unit.Core.Application.Alerts;

public class PortfolioDrawdownAlertEvaluatorTests : IDisposable
{
    private static readonly DateOnly Today = new(2026, 4, 26);
    private static readonly DateOnly AthDate = new(2026, 1, 15);

    private readonly HoardContext _context;
    private readonly Mock<IAlertService> _alertService = new();
    private readonly PortfolioDrawdownAlertEvaluator _evaluator;

    public PortfolioDrawdownAlertEvaluatorTests()
    {
        _context = TestContextFactory.Create();
        _evaluator = new PortfolioDrawdownAlertEvaluator(_alertService.Object);
    }

    private async Task<Portfolio> SeedPortfolio(decimal? allTimeHighValue, DateOnly? allTimeHighDate)
    {
        var portfolio = TestData.Portfolio();
        portfolio.AllTimeHighValue = allTimeHighValue;
        portfolio.AllTimeHighDate = allTimeHighDate;
        _context.Portfolios.Add(portfolio);
        await _context.SaveChangesAsync();
        return portfolio;
    }

    private static PortfolioPerformance Performance(Portfolio portfolio, decimal value) =>
        new() { PortfolioId = portfolio.Id, Portfolio = portfolio, Value = value };

    [Fact]
    public async Task EvaluateAsync_NoAllTimeHigh_DoesNotRaiseAnyAlert()
    {
        var portfolio = await SeedPortfolio(null, null);

        await _evaluator.EvaluateAsync(Performance(portfolio, 9000m), Today, CancellationToken.None);

        _alertService.Verify(a => a.TryRaiseAsync(It.IsAny<AlertRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task EvaluateAsync_DrawdownBelow10Percent_DoesNotRaiseAnyAlert()
    {
        var portfolio = await SeedPortfolio(10000m, AthDate);

        await _evaluator.EvaluateAsync(Performance(portfolio, 9500m), Today, CancellationToken.None);

        _alertService.Verify(a => a.TryRaiseAsync(It.IsAny<AlertRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task EvaluateAsync_DrawdownExactly10Percent_RaisesOnlyDrawdown10Alert()
    {
        var portfolio = await SeedPortfolio(10000m, AthDate);
        _alertService
            .Setup(a => a.TryRaiseAsync(It.IsAny<AlertRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await _evaluator.EvaluateAsync(Performance(portfolio, 9000m), Today, CancellationToken.None);

        _alertService.Verify(a => a.TryRaiseAsync(
            It.Is<AlertRequest>(r =>
                r.AlertTypeId == AlertType.PortfolioDrawdown10 &&
                r.PortfolioId == portfolio.Id &&
                r.TriggeredDate == Today &&
                r.SuppressionKey == $"{AlertType.PortfolioDrawdown10}:{portfolio.Id}:2026-01-15"),
            It.IsAny<CancellationToken>()), Times.Once);

        _alertService.Verify(a => a.TryRaiseAsync(
            It.Is<AlertRequest>(r => r.AlertTypeId == AlertType.PortfolioDrawdown20),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EvaluateAsync_DrawdownAbove20Percent_RaisesDrawdown10And20Alerts()
    {
        var portfolio = await SeedPortfolio(10000m, AthDate);
        _alertService
            .Setup(a => a.TryRaiseAsync(It.IsAny<AlertRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await _evaluator.EvaluateAsync(Performance(portfolio, 7500m), Today, CancellationToken.None);

        _alertService.Verify(a => a.TryRaiseAsync(
            It.Is<AlertRequest>(r => r.AlertTypeId == AlertType.PortfolioDrawdown10),
            It.IsAny<CancellationToken>()), Times.Once);

        _alertService.Verify(a => a.TryRaiseAsync(
            It.Is<AlertRequest>(r => r.AlertTypeId == AlertType.PortfolioDrawdown20),
            It.IsAny<CancellationToken>()), Times.Once);

        _alertService.Verify(a => a.TryRaiseAsync(
            It.Is<AlertRequest>(r => r.AlertTypeId == AlertType.PortfolioDrawdown30),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EvaluateAsync_DrawdownAbove50Percent_RaisesAllFourAlerts()
    {
        var portfolio = await SeedPortfolio(10000m, AthDate);
        _alertService
            .Setup(a => a.TryRaiseAsync(It.IsAny<AlertRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await _evaluator.EvaluateAsync(Performance(portfolio, 4000m), Today, CancellationToken.None);

        _alertService.Verify(a => a.TryRaiseAsync(
            It.Is<AlertRequest>(r => r.AlertTypeId == AlertType.PortfolioDrawdown10),
            It.IsAny<CancellationToken>()), Times.Once);

        _alertService.Verify(a => a.TryRaiseAsync(
            It.Is<AlertRequest>(r => r.AlertTypeId == AlertType.PortfolioDrawdown20),
            It.IsAny<CancellationToken>()), Times.Once);

        _alertService.Verify(a => a.TryRaiseAsync(
            It.Is<AlertRequest>(r => r.AlertTypeId == AlertType.PortfolioDrawdown30),
            It.IsAny<CancellationToken>()), Times.Once);

        _alertService.Verify(a => a.TryRaiseAsync(
            It.Is<AlertRequest>(r => r.AlertTypeId == AlertType.PortfolioDrawdown50),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EvaluateAsync_SuppressionKeyEncodesAthDate()
    {
        var portfolio = await SeedPortfolio(10000m, AthDate);
        _alertService
            .Setup(a => a.TryRaiseAsync(It.IsAny<AlertRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await _evaluator.EvaluateAsync(Performance(portfolio, 8000m), Today, CancellationToken.None);

        _alertService.Verify(a => a.TryRaiseAsync(
            It.Is<AlertRequest>(r =>
                r.AlertTypeId == AlertType.PortfolioDrawdown20 &&
                r.SuppressionKey == $"{AlertType.PortfolioDrawdown20}:{portfolio.Id}:2026-01-15"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EvaluateAsync_NewAthDate_SuppressionKeyEncodesNewDate()
    {
        var newAthDate = new DateOnly(2026, 3, 1);
        var portfolio = await SeedPortfolio(12000m, newAthDate);
        _alertService
            .Setup(a => a.TryRaiseAsync(It.IsAny<AlertRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // 25% drawdown: crosses 10% and 20% but not 30%
        await _evaluator.EvaluateAsync(Performance(portfolio, 9000m), Today, CancellationToken.None);

        _alertService.Verify(a => a.TryRaiseAsync(
            It.Is<AlertRequest>(r =>
                r.AlertTypeId == AlertType.PortfolioDrawdown20 &&
                r.SuppressionKey == $"{AlertType.PortfolioDrawdown20}:{portfolio.Id}:2026-03-01"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EvaluateAsync_AlertSuppressed_DoesNotThrow()
    {
        var portfolio = await SeedPortfolio(10000m, AthDate);
        _alertService
            .Setup(a => a.TryRaiseAsync(It.IsAny<AlertRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await _evaluator.EvaluateAsync(Performance(portfolio, 8000m), Today, CancellationToken.None);

        _alertService.Verify(a => a.TryRaiseAsync(It.IsAny<AlertRequest>(), It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
