using Hoard.Core.Application.Alerts;
using Hoard.Core.Domain.Entities;
using Hoard.Test.Shared;
using Moq;

namespace Hoard.Test.Unit.Core.Application.Alerts;

public class CloseOfDayValuationAlertEvaluatorTests
{
    private static readonly DateOnly Today = new(2026, 4, 8);
    private readonly Mock<IAlertService> _alertService = new();

    [Fact]
    public async Task EvaluateAsync_RaisesCloseOfDayAlert()
    {
        var evaluator = new CloseOfDayValuationAlertEvaluator(_alertService.Object);
        var portfolio = TestData.Portfolio(id: 1, name: "ISA");
        var performance = new PortfolioPerformance
        {
            PortfolioId = 1,
            Portfolio = portfolio,
            Value = 10000m,
            ValueChange1D = 100m,
            Return1D = 1.0m,
            Return1Y = 10.0m
        };

        await evaluator.EvaluateAsync(performance, Today, CancellationToken.None);

        _alertService.Verify(a => a.TryRaiseAsync(It.Is<AlertRequest>(r =>
            r.AlertTypeId == AlertType.CloseOfDayPortfolioValuation &&
            r.PortfolioId == 1 &&
            r.TriggeredDate == Today &&
            r.SuppressionKey == $"{AlertType.CloseOfDayPortfolioValuation}:1:2026-04-08" &&
            r.Title == "ISA close of day 2026-04-08" &&
            r.Url == "http://hoard.iannelson.uk/portfolios/1/overview" &&
            r.UrlTitle == "ISA"),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
