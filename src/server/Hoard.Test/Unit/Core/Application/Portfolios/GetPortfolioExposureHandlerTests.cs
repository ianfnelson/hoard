using Hoard.Core;
using Hoard.Core.Application.Portfolios;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace Hoard.Test.Unit.Core.Application.Portfolios;

public class GetPortfolioExposureHandlerTests
{
    private readonly Mock<ILogger<GetPortfolioExposureHandler>> _mockLogger;
    private readonly IRebalanceService _rebalanceService;

    public GetPortfolioExposureHandlerTests()
    {
        _mockLogger = new Mock<ILogger<GetPortfolioExposureHandler>>();
        _rebalanceService = new RebalanceService();
    }

    [Fact]
    public async Task HandleAsync_PortfolioNotFound_ReturnsNull()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var handler = new GetPortfolioExposureHandler(context, _mockLogger.Object, _rebalanceService);

        // Act
        var result = await handler.HandleAsync(new GetPortfolioExposureQuery(999));

        // Assert
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
        // Arrange
        await using var context = CreateInMemoryContext();
        var handler = new GetPortfolioExposureHandler(context, _mockLogger.Object, _rebalanceService);

        var portfolio = new Portfolio { Id = 1, Name = "Test Portfolio" };
        context.Portfolios.Add(portfolio);
        await context.SaveChangesAsync();

        // Act
        var result = await handler.HandleAsync(new GetPortfolioExposureQuery(1));

        // Assert
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
        // Arrange
        await using var context = CreateInMemoryContext();
        var handler = new GetPortfolioExposureHandler(context, _mockLogger.Object, _rebalanceService);

        var today = DateOnlyHelper.TodayLocal();

        // Setup taxonomy
        var assetClass = new AssetClass { Id = 1, Name = "Equity", Colour = "#FF0000" };
        var assetSubclass = new AssetSubclass { Id = 10, Name = "Stocks", AssetClassId = 1, Colour = "#FF5555" };
        context.AssetClasses.Add(assetClass);
        context.AssetSubclasses.Add(assetSubclass);

        // Setup portfolio and account
        var account = new Account { Id = 1, Name = "Test Account", Colour = "#000000" };
        var portfolio = new Portfolio { Id = 1, Name = "Test Portfolio" };
        portfolio.Accounts.Add(account);
        context.Portfolios.Add(portfolio);

        // Setup instrument and holding
        var instrumentType = new InstrumentType { Id = 1, Name = "Stock", Colour = "#000000" };
        var currency = new Currency { Id = "GBP", Name = "British Pound" };
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
        context.InstrumentTypes.Add(instrumentType);
        context.Currencies.Add(currency);
        context.Instruments.Add(instrument);

        var holding = new Holding
        {
            Id = 1,
            AccountId = 1,
            InstrumentId = 1,
            AsOfDate = today
        };
        context.Holdings.Add(holding);

        // Setup valuation
        var valuation = new HoldingValuation
        {
            Id = 1,
            HoldingId = 1,
            Value = 10000m
        };
        context.HoldingValuations.Add(valuation);

        // Setup target allocation
        var targetAllocation = new TargetAllocation
        {
            PortfolioId = 1,
            AssetSubclassId = 10,
            Target = 100m
        };
        context.TargetAllocations.Add(targetAllocation);

        await context.SaveChangesAsync();

        // Act
        var result = await handler.HandleAsync(new GetPortfolioExposureQuery(1));

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.PortfolioId);
        Assert.Equal(10000m, result.TotalValue);

        // Check asset class exposure
        var assetClassExposure = Assert.Single(result.AssetClasses);
        Assert.Equal(1, assetClassExposure.Id);
        Assert.Equal("Equity", assetClassExposure.Name);
        Assert.Equal(10000m, assetClassExposure.Value);
        Assert.Equal(100m, assetClassExposure.Percentage);
        Assert.Equal(10000m, assetClassExposure.TargetValue);
        Assert.Equal(100m, assetClassExposure.TargetPercentage);
        Assert.Equal(0m, assetClassExposure.DeviationValue);
        Assert.Equal(0m, assetClassExposure.DeviationPercentage);

        // Check asset subclass exposure
        var subclassExposure = Assert.Single(result.AssetSubclasses);
        Assert.Equal(10, subclassExposure.Id);
        Assert.Equal("Stocks", subclassExposure.Name);
        Assert.Equal(10000m, subclassExposure.Value);
        Assert.Equal(100m, subclassExposure.Percentage);
    }

    [Fact]
    public async Task HandleAsync_MultipleSubclasses_CalculatesExposureCorrectly()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var handler = new GetPortfolioExposureHandler(context, _mockLogger.Object, _rebalanceService);

        var today = DateOnlyHelper.TodayLocal();

        // Setup taxonomy
        var equityClass = new AssetClass { Id = 1, Name = "Equity", Colour = "#FF0000" };
        var bondClass = new AssetClass { Id = 2, Name = "Fixed Income", Colour = "#00FF00" };
        var stocksSubclass = new AssetSubclass { Id = 10, Name = "Stocks", AssetClassId = 1, Colour = "#FF5555" };
        var bondsSubclass = new AssetSubclass { Id = 20, Name = "Bonds", AssetClassId = 2, Colour = "#55FF55" };
        context.AssetClasses.AddRange(equityClass, bondClass);
        context.AssetSubclasses.AddRange(stocksSubclass, bondsSubclass);

        // Setup portfolio and account
        var account = new Account { Id = 1, Name = "Test Account", Colour = "#000000" };
        var portfolio = new Portfolio { Id = 1, Name = "Test Portfolio" };
        portfolio.Accounts.Add(account);
        context.Portfolios.Add(portfolio);

        var instrumentType = new InstrumentType { Id = 1, Name = "Fund", Colour = "#000000" };
        var currency = new Currency { Id = "GBP", Name = "British Pound" };
        context.InstrumentTypes.Add(instrumentType);
        context.Currencies.Add(currency);

        var stockInstrument = new Instrument
        {
            Id = 1,
            Name = "Stock Fund",
            Ticker = "STK",
            AssetSubclassId = 10,
            InstrumentTypeId = 1,
            CurrencyId = "GBP"
        };
        var bondInstrument = new Instrument
        {
            Id = 2,
            Name = "Bond Fund",
            Ticker = "BND",
            AssetSubclassId = 20,
            InstrumentTypeId = 1,
            CurrencyId = "GBP"
        };
        context.Instruments.AddRange(stockInstrument, bondInstrument);

        // Setup holdings and valuations
        var stockHolding = new Holding { Id = 1, AccountId = 1, InstrumentId = 1, AsOfDate = today };
        var bondHolding = new Holding { Id = 2, AccountId = 1, InstrumentId = 2, AsOfDate = today };
        context.Holdings.AddRange(stockHolding, bondHolding);
        context.HoldingValuations.AddRange(
            new HoldingValuation { Id = 1, HoldingId = 1, Value = 60000m },
            new HoldingValuation { Id = 2, HoldingId = 2, Value = 40000m }
        );

        // Setup target allocations (50/50 split)
        context.TargetAllocations.AddRange(
            new TargetAllocation { PortfolioId = 1, AssetSubclassId = 10, Target = 50m },
            new TargetAllocation { PortfolioId = 1, AssetSubclassId = 20, Target = 50m }
        );

        await context.SaveChangesAsync();

        // Act
        var result = await handler.HandleAsync(new GetPortfolioExposureQuery(1));

        // Assert
        Assert.NotNull(result);
        Assert.Equal(100000m, result.TotalValue);

        // Check subclass exposure
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

        // Check asset class roll-up
        Assert.Equal(2, result.AssetClasses.Count);
        var equityExposure = result.AssetClasses.First(x => x.Id == 1);
        Assert.Equal(60000m, equityExposure.Value);
        Assert.Equal(60m, equityExposure.Percentage);
    }

    [Fact]
    public async Task HandleAsync_OrdersSubclassesByPercentageDescending()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var handler = new GetPortfolioExposureHandler(context, _mockLogger.Object, _rebalanceService);

        var today = DateOnlyHelper.TodayLocal();

        // Setup taxonomy
        var assetClass = new AssetClass { Id = 1, Name = "Mixed", Colour = "#000000" };
        context.AssetClasses.Add(assetClass);
        context.AssetSubclasses.AddRange(
            new AssetSubclass { Id = 1, Name = "Large Holdings", AssetClassId = 1, Colour = "#111" },
            new AssetSubclass { Id = 2, Name = "Medium Holdings", AssetClassId = 1, Colour = "#222" },
            new AssetSubclass { Id = 3, Name = "Small Holdings", AssetClassId = 1, Colour = "#333" }
        );

        var account = new Account { Id = 1, Name = "Test Account", Colour = "#000000" };
        var portfolio = new Portfolio { Id = 1, Name = "Test Portfolio" };
        portfolio.Accounts.Add(account);
        context.Portfolios.Add(portfolio);

        var instrumentType = new InstrumentType { Id = 1, Name = "Fund", Colour = "#000000" };
        var currency = new Currency { Id = "GBP", Name = "British Pound" };
        context.InstrumentTypes.Add(instrumentType);
        context.Currencies.Add(currency);

        // Create instruments with different values
        for (int i = 1; i <= 3; i++)
        {
            context.Instruments.Add(new Instrument
            {
                Id = i,
                Name = $"Instrument {i}",
                Ticker = $"TST{i}",
                AssetSubclassId = i,
                InstrumentTypeId = 1,
                CurrencyId = "GBP"
            });

            var holding = new Holding { Id = i, AccountId = 1, InstrumentId = i, AsOfDate = today };
            context.Holdings.Add(holding);
        }

        // Values: Large=50k, Medium=30k, Small=20k
        context.HoldingValuations.AddRange(
            new HoldingValuation { Id = 1, HoldingId = 1, Value = 50000m },
            new HoldingValuation { Id = 2, HoldingId = 2, Value = 30000m },
            new HoldingValuation { Id = 3, HoldingId = 3, Value = 20000m }
        );

        await context.SaveChangesAsync();

        // Act
        var result = await handler.HandleAsync(new GetPortfolioExposureQuery(1));

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.AssetSubclasses.Count);

        // Should be ordered by percentage descending
        Assert.Equal("Large Holdings", result.AssetSubclasses[0].Name);
        Assert.Equal("Medium Holdings", result.AssetSubclasses[1].Name);
        Assert.Equal("Small Holdings", result.AssetSubclasses[2].Name);
    }

    private static HoardContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<HoardContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new HoardContext(options);
    }
}
