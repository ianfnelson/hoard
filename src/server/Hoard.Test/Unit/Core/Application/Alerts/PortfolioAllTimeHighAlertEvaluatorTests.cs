using Hoard.Core.Application.Alerts;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Test.Shared;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Hoard.Test.Unit.Core.Application.Alerts;

public class PortfolioAllTimeHighAlertEvaluatorTests : IDisposable
{
    private static readonly DateOnly Today = new(2026, 4, 8);
    private static readonly DateOnly PriorDate = new(2026, 3, 9);

    private readonly HoardContext _context;
    private readonly Mock<IAlertService> _alertService = new();
    private readonly PortfolioAllTimeHighAlertEvaluator _evaluator;

    public PortfolioAllTimeHighAlertEvaluatorTests()
    {
        _context = TestContextFactory.Create();
        _evaluator = new PortfolioAllTimeHighAlertEvaluator(_context, _alertService.Object);
    }

    private async Task<Portfolio> SeedPortfolio(decimal? storedHigh, DateOnly? storedDate)
    {
        var portfolio = TestData.Portfolio();
        portfolio.AllTimeHighValue = storedHigh;
        portfolio.AllTimeHighDate = storedDate;
        _context.Portfolios.Add(portfolio);
        await _context.SaveChangesAsync();
        return portfolio;
    }

    private static PortfolioPerformance Performance(Portfolio portfolio, decimal value) =>
        new() { PortfolioId = portfolio.Id, Portfolio = portfolio, Value = value };

    [Fact]
    public async Task EvaluateAsync_FirstEverValuation_RaisesAlertAndStoresHigh()
    {
        var portfolio = await SeedPortfolio(null, null);
        _alertService
            .Setup(a => a.TryRaiseAsync(It.IsAny<AlertRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await _evaluator.EvaluateAsync(Performance(portfolio, 12345m), Today, CancellationToken.None);

        _alertService.Verify(a => a.TryRaiseAsync(It.Is<AlertRequest>(r =>
            r.AlertTypeId == AlertType.PortfolioAllTimeHigh &&
            r.PortfolioId == portfolio.Id &&
            r.TriggeredDate == Today &&
            r.SuppressionKey == $"{AlertType.PortfolioAllTimeHigh}:{portfolio.Id}:2026-04-08" &&
            r.Url == $"http://hoard.iannelson.uk/portfolios/{portfolio.Id}/overview"),
            It.IsAny<CancellationToken>()), Times.Once);

        var stored = await _context.Portfolios.AsNoTracking().SingleAsync(p => p.Id == portfolio.Id);
        Assert.Equal(12345m, stored.AllTimeHighValue);
        Assert.Equal(Today, stored.AllTimeHighDate);
    }

    [Fact]
    public async Task EvaluateAsync_NewHigh_RaisesAlertAndUpdatesStoredHigh()
    {
        var portfolio = await SeedPortfolio(10000m, PriorDate);
        _alertService
            .Setup(a => a.TryRaiseAsync(It.IsAny<AlertRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await _evaluator.EvaluateAsync(Performance(portfolio, 11000m), Today, CancellationToken.None);

        _alertService.Verify(a => a.TryRaiseAsync(It.Is<AlertRequest>(r =>
            r.AlertTypeId == AlertType.PortfolioAllTimeHigh), It.IsAny<CancellationToken>()), Times.Once);

        var stored = await _context.Portfolios.AsNoTracking().SingleAsync(p => p.Id == portfolio.Id);
        Assert.Equal(11000m, stored.AllTimeHighValue);
        Assert.Equal(Today, stored.AllTimeHighDate);
    }

    [Fact]
    public async Task EvaluateAsync_BelowStoredHigh_DoesNotRaiseOrUpdate()
    {
        var portfolio = await SeedPortfolio(10000m, PriorDate);

        await _evaluator.EvaluateAsync(Performance(portfolio, 9500m), Today, CancellationToken.None);

        _alertService.Verify(a => a.TryRaiseAsync(It.IsAny<AlertRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);

        var stored = await _context.Portfolios.AsNoTracking().SingleAsync(p => p.Id == portfolio.Id);
        Assert.Equal(10000m, stored.AllTimeHighValue);
        Assert.Equal(PriorDate, stored.AllTimeHighDate);
    }

    [Fact]
    public async Task EvaluateAsync_EqualToStoredHigh_DoesNotRaiseOrUpdate()
    {
        var portfolio = await SeedPortfolio(10000m, PriorDate);

        await _evaluator.EvaluateAsync(Performance(portfolio, 10000m), Today, CancellationToken.None);

        _alertService.Verify(a => a.TryRaiseAsync(It.IsAny<AlertRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);

        var stored = await _context.Portfolios.AsNoTracking().SingleAsync(p => p.Id == portfolio.Id);
        Assert.Equal(10000m, stored.AllTimeHighValue);
        Assert.Equal(PriorDate, stored.AllTimeHighDate);
    }

    [Fact]
    public async Task EvaluateAsync_AlertSuppressed_DoesNotUpdateStoredHigh()
    {
        var portfolio = await SeedPortfolio(10000m, PriorDate);
        _alertService
            .Setup(a => a.TryRaiseAsync(It.IsAny<AlertRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await _evaluator.EvaluateAsync(Performance(portfolio, 11000m), Today, CancellationToken.None);

        var stored = await _context.Portfolios.AsNoTracking().SingleAsync(p => p.Id == portfolio.Id);
        Assert.Equal(10000m, stored.AllTimeHighValue);
        Assert.Equal(PriorDate, stored.AllTimeHighDate);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
