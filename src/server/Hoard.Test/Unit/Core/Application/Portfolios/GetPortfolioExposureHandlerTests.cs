using Hoard.Core;
using Hoard.Core.Application.Portfolios;
using Hoard.Core.Domain.Entities;
using Hoard.Test.Shared;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace Hoard.Test.Unit.Core.Application.Portfolios;

public class GetPortfolioExposureHandlerTests
{
    private static readonly DateOnly Today = new(2024, 6, 15);

    private readonly Mock<ILogger<GetPortfolioExposureHandler>> _mockLogger;
    private readonly IRebalanceService _rebalanceService;
    private readonly FakeTimeProvider _timeProvider = new();

    public GetPortfolioExposureHandlerTests()
    {
        _mockLogger = new Mock<ILogger<GetPortfolioExposureHandler>>();
        _rebalanceService = new RebalanceService();

        var ukNoon = new DateTime(Today.Year, Today.Month, Today.Day, 12, 0, 0, DateTimeKind.Unspecified);
        var utc = TimeZoneInfo.ConvertTimeToUtc(ukNoon, DateOnlyHelper.UkTimeZone);
        _timeProvider.SetUtcNow(new DateTimeOffset(utc, TimeSpan.Zero));
    }

    private GetPortfolioExposureHandler CreateHandler(Hoard.Core.Data.HoardContext context)
        => new(context, _mockLogger.Object, _rebalanceService, _timeProvider);

    [Fact]
    public async Task HandleAsync_PortfolioNotFound_ReturnsNull()
    {
        await using var context = TestContextFactory.Create();
        var handler = CreateHandler(context);

        var result = await handler.HandleAsync(new GetPortfolioExposureQuery(999));

        Assert.Null(result);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_EmptyPortfolio_ReturnsZeroValues()
    {
        await using var context = TestContextFactory.Create();
        var handler = CreateHandler(context);

        context.Portfolios.Add(TestData.Portfolio());
        await context.SaveChangesAsync();

        var result = await handler.HandleAsync(new GetPortfolioExposureQuery(1));

        Assert.NotNull(result);
        Assert.Equal(1, result.PortfolioId);
        Assert.Equal(0m, result.TotalValue);
        Assert.Empty(result.AssetClasses);
        Assert.Empty(result.AssetSubclasses);
        Assert.Empty(result.RebalanceActions);
    }

    [Fact]
    public async Task HandleAsync_SingleHolding_ReturnsCorrectExposure()
    {
        await using var context = TestContextFactory.Create();
        var handler = CreateHandler(context);

        context.AssetClasses.Add(TestData.AssetClass(1, "Equity"));
        context.AssetSubclasses.Add(TestData.AssetSubclass(10, "Stocks", 1));
        context.InstrumentTypes.Add(TestData.InstrumentType());
        context.Currencies.Add(TestData.Currency());

        var account = TestData.Account();
        var portfolio = TestData.Portfolio();
        portfolio.Accounts.Add(account);
        context.Portfolios.Add(portfolio);

        var instrument = new Instrument
        {
            Id = 1,
            Name = "Test Stock",
            Ticker = "TST",
            Isin = "GB0000000000",
            AssetSubclassId = 10,
            InstrumentTypeId = 1,
            CurrencyId = "GBP"
        };
        context.Instruments.Add(instrument);

        context.Holdings.Add(new Holding { Id = 1, AccountId = 1, InstrumentId = 1, AsOfDate = Today });
        context.HoldingValuations.Add(new HoldingValuation { Id = 1, HoldingId = 1, Value = 10000m });
        context.TargetAllocations.Add(new TargetAllocation { PortfolioId = 1, AssetSubclassId = 10, Target = 100m });

        await context.SaveChangesAsync();

        var result = await handler.HandleAsync(new GetPortfolioExposureQuery(1));

        Assert.NotNull(result);
        Assert.Equal(1, result.PortfolioId);
        Assert.Equal(10000m, result.TotalValue);

        var assetClassExposure = Assert.Single(result.AssetClasses);
        Assert.Equal(1, assetClassExposure.Id);
        Assert.Equal("Equity", assetClassExposure.Name);
        Assert.Equal(10000m, assetClassExposure.Value);
        Assert.Equal(100m, assetClassExposure.Percentage);
        Assert.Equal(10000m, assetClassExposure.TargetValue);
        Assert.Equal(100m, assetClassExposure.TargetPercentage);
        Assert.Equal(0m, assetClassExposure.DeviationValue);
        Assert.Equal(0m, assetClassExposure.DeviationPercentage);

        var subclassExposure = Assert.Single(result.AssetSubclasses);
        Assert.Equal(10, subclassExposure.Id);
        Assert.Equal("Stocks", subclassExposure.Name);
        Assert.Equal(10000m, subclassExposure.Value);
        Assert.Equal(100m, subclassExposure.Percentage);
    }

    [Fact]
    public async Task HandleAsync_MultipleSubclasses_CalculatesExposureCorrectly()
    {
        await using var context = TestContextFactory.Create();
        var handler = CreateHandler(context);

        context.AssetClasses.AddRange(
            TestData.AssetClass(1, "Equity"),
            TestData.AssetClass(2, "Fixed Income", "#00FF00"));
        context.AssetSubclasses.AddRange(
            TestData.AssetSubclass(10, "Stocks", 1),
            TestData.AssetSubclass(20, "Bonds", 2, "#55FF55"));
        context.InstrumentTypes.Add(TestData.InstrumentType());
        context.Currencies.Add(TestData.Currency());

        var account = TestData.Account();
        var portfolio = TestData.Portfolio();
        portfolio.Accounts.Add(account);
        context.Portfolios.Add(portfolio);

        context.Instruments.AddRange(
            new Instrument { Id = 1, Name = "Stock Fund", Ticker = "STK", AssetSubclassId = 10, InstrumentTypeId = 1, CurrencyId = "GBP" },
            new Instrument { Id = 2, Name = "Bond Fund", Ticker = "BND", AssetSubclassId = 20, InstrumentTypeId = 1, CurrencyId = "GBP" });

        context.Holdings.AddRange(
            new Holding { Id = 1, AccountId = 1, InstrumentId = 1, AsOfDate = Today },
            new Holding { Id = 2, AccountId = 1, InstrumentId = 2, AsOfDate = Today });
        context.HoldingValuations.AddRange(
            new HoldingValuation { Id = 1, HoldingId = 1, Value = 60000m },
            new HoldingValuation { Id = 2, HoldingId = 2, Value = 40000m });

        context.TargetAllocations.AddRange(
            new TargetAllocation { PortfolioId = 1, AssetSubclassId = 10, Target = 50m },
            new TargetAllocation { PortfolioId = 1, AssetSubclassId = 20, Target = 50m });

        await context.SaveChangesAsync();

        var result = await handler.HandleAsync(new GetPortfolioExposureQuery(1));

        Assert.NotNull(result);
        Assert.Equal(100000m, result.TotalValue);

        Assert.Equal(2, result.AssetSubclasses.Count);

        var stocksExposure = result.AssetSubclasses.First(x => x.Id == 10);
        Assert.Equal(60000m, stocksExposure.Value);
        Assert.Equal(60m, stocksExposure.Percentage);
        Assert.Equal(50000m, stocksExposure.TargetValue);
        Assert.Equal(50m, stocksExposure.TargetPercentage);
        Assert.Equal(10000m, stocksExposure.DeviationValue);
        Assert.Equal(20m, stocksExposure.DeviationPercentage);

        var bondsExposure = result.AssetSubclasses.First(x => x.Id == 20);
        Assert.Equal(40000m, bondsExposure.Value);
        Assert.Equal(40m, bondsExposure.Percentage);
        Assert.Equal(50000m, bondsExposure.TargetValue);
        Assert.Equal(50m, bondsExposure.TargetPercentage);
        Assert.Equal(-10000m, bondsExposure.DeviationValue);
        Assert.Equal(-20m, bondsExposure.DeviationPercentage);

        Assert.Equal(2, result.AssetClasses.Count);
        var equityExposure = result.AssetClasses.First(x => x.Id == 1);
        Assert.Equal(60000m, equityExposure.Value);
        Assert.Equal(60m, equityExposure.Percentage);
    }

    [Fact]
    public async Task HandleAsync_OrdersSubclassesByPercentageDescending()
    {
        await using var context = TestContextFactory.Create();
        var handler = CreateHandler(context);

        context.AssetClasses.Add(TestData.AssetClass(1, "Mixed", "#000000"));
        context.AssetSubclasses.AddRange(
            TestData.AssetSubclass(1, "Large Holdings", 1, "#111"),
            TestData.AssetSubclass(2, "Medium Holdings", 1, "#222"),
            TestData.AssetSubclass(3, "Small Holdings", 1, "#333"));
        context.InstrumentTypes.Add(TestData.InstrumentType());
        context.Currencies.Add(TestData.Currency());

        var account = TestData.Account();
        var portfolio = TestData.Portfolio();
        portfolio.Accounts.Add(account);
        context.Portfolios.Add(portfolio);

        for (var i = 1; i <= 3; i++)
        {
            context.Instruments.Add(new Instrument
            {
                Id = i, Name = $"Instrument {i}", Ticker = $"TST{i}",
                AssetSubclassId = i, InstrumentTypeId = 1, CurrencyId = "GBP"
            });
            context.Holdings.Add(new Holding { Id = i, AccountId = 1, InstrumentId = i, AsOfDate = Today });
        }

        context.HoldingValuations.AddRange(
            new HoldingValuation { Id = 1, HoldingId = 1, Value = 50000m },
            new HoldingValuation { Id = 2, HoldingId = 2, Value = 30000m },
            new HoldingValuation { Id = 3, HoldingId = 3, Value = 20000m });

        await context.SaveChangesAsync();

        var result = await handler.HandleAsync(new GetPortfolioExposureQuery(1));

        Assert.NotNull(result);
        Assert.Equal(3, result.AssetSubclasses.Count);
        Assert.Equal("Large Holdings", result.AssetSubclasses[0].Name);
        Assert.Equal("Medium Holdings", result.AssetSubclasses[1].Name);
        Assert.Equal("Small Holdings", result.AssetSubclasses[2].Name);
    }
}
