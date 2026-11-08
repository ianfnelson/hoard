using Hoard.Core.Application.Instruments;
using Hoard.Core.Domain.Entities;
using Hoard.Test.Shared;

namespace Hoard.Test.Unit.Core.Application.Instruments;

public class GetInstrumentsHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsPagedResults()
    {
        // Arrange
        await using var context = TestContextFactory.Create();
        TestData.SeedReferenceData(context);
        for (var i = 1; i <= 5; i++)
        {
            context.Instruments.Add(new Instrument
            {
                Id = i, Name = $"Stock {i}", Ticker = $"S{i}",
                CurrencyId = "GBP", InstrumentTypeId = 1, AssetSubclassId = 1
            });
        }
        await context.SaveChangesAsync();

        var handler = new GetInstrumentsHandler(context);
        var query = new GetInstrumentsQuery { PageNumber = 1, PageSize = 3 };

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(3, result.Items.Count);
    }

    [Fact]
    public async Task HandleAsync_ExcludesCashInstruments()
    {
        // Arrange
        await using var context = TestContextFactory.Create();
        TestData.SeedReferenceData(context);
        context.Instruments.AddRange(
            new Instrument { Id = 1, Name = "Stock", Ticker = "STK", CurrencyId = "GBP", InstrumentTypeId = 1, AssetSubclassId = 1 },
            new Instrument { Id = 2, Name = "Cash GBP", Ticker = "GBP", CurrencyId = "GBP", InstrumentTypeId = InstrumentType.Cash, AssetSubclassId = 1 }
        );
        await context.SaveChangesAsync();

        var handler = new GetInstrumentsHandler(context);

        // Act
        var result = await handler.HandleAsync(new GetInstrumentsQuery());

        // Assert
        Assert.Equal(1, result.TotalCount);
        Assert.Equal("Stock", result.Items[0].Name);
    }

    [Fact]
    public async Task HandleAsync_FiltersByInstrumentTypeId()
    {
        // Arrange
        await using var context = TestContextFactory.Create();
        TestData.SeedReferenceData(context);
        context.InstrumentTypes.Add(TestData.InstrumentType(id: 2, name: "ETF", colour: "#00FF00"));
        context.Instruments.AddRange(
            new Instrument { Id = 1, Name = "Stock A", Ticker = "SA", CurrencyId = "GBP", InstrumentTypeId = 1, AssetSubclassId = 1 },
            new Instrument { Id = 2, Name = "ETF B", Ticker = "EB", CurrencyId = "GBP", InstrumentTypeId = 2, AssetSubclassId = 1 }
        );
        await context.SaveChangesAsync();

        var handler = new GetInstrumentsHandler(context);

        // Act
        var result = await handler.HandleAsync(new GetInstrumentsQuery { InstrumentTypeId = 2 });

        // Assert
        Assert.Equal(1, result.TotalCount);
        Assert.Equal("ETF B", result.Items[0].Name);
    }

    [Fact]
    public async Task HandleAsync_SearchByNameOrTicker()
    {
        // Arrange
        await using var context = TestContextFactory.Create();
        TestData.SeedReferenceData(context);
        context.Instruments.AddRange(
            new Instrument { Id = 1, Name = "Fundsmith Equity", Ticker = "FUND", CurrencyId = "GBP", InstrumentTypeId = 1, AssetSubclassId = 1 },
            new Instrument { Id = 2, Name = "Vanguard FTSE", Ticker = "VUKE", CurrencyId = "GBP", InstrumentTypeId = 1, AssetSubclassId = 1 }
        );
        await context.SaveChangesAsync();

        var handler = new GetInstrumentsHandler(context);

        // Act
        var result = await handler.HandleAsync(new GetInstrumentsQuery { Search = "FUND" });

        // Assert
        Assert.Equal(1, result.TotalCount);
        Assert.Equal("Fundsmith Equity", result.Items[0].Name);
    }

    [Fact]
    public async Task HandleAsync_DefaultSortByNameAscending()
    {
        // Arrange
        await using var context = TestContextFactory.Create();
        TestData.SeedReferenceData(context);
        context.Instruments.AddRange(
            new Instrument { Id = 1, Name = "Zebra Corp", Ticker = "ZEB", CurrencyId = "GBP", InstrumentTypeId = 1, AssetSubclassId = 1 },
            new Instrument { Id = 2, Name = "Alpha Inc", Ticker = "ALP", CurrencyId = "GBP", InstrumentTypeId = 1, AssetSubclassId = 1 }
        );
        await context.SaveChangesAsync();

        var handler = new GetInstrumentsHandler(context);

        // Act
        var result = await handler.HandleAsync(new GetInstrumentsQuery());

        // Assert
        Assert.Equal("Alpha Inc", result.Items[0].Name);
        Assert.Equal("Zebra Corp", result.Items[1].Name);
    }

    [Fact]
    public async Task HandleAsync_SortByTickerDescending()
    {
        // Arrange
        await using var context = TestContextFactory.Create();
        TestData.SeedReferenceData(context);
        context.Instruments.AddRange(
            new Instrument { Id = 1, Name = "Alpha Inc", Ticker = "AAA", CurrencyId = "GBP", InstrumentTypeId = 1, AssetSubclassId = 1 },
            new Instrument { Id = 2, Name = "Zebra Corp", Ticker = "MMM", CurrencyId = "GBP", InstrumentTypeId = 1, AssetSubclassId = 1 },
            new Instrument { Id = 3, Name = "Middle Ltd", Ticker = "ZZZ", CurrencyId = "GBP", InstrumentTypeId = 1, AssetSubclassId = 1 }
        );
        await context.SaveChangesAsync();

        var handler = new GetInstrumentsHandler(context);

        // Act
        var result = await handler.HandleAsync(new GetInstrumentsQuery { SortBy = "ticker", SortDirection = "desc" });

        // Assert
        Assert.Equal(["ZZZ", "MMM", "AAA"], result.Items.Select(i => i.Ticker));
    }

    [Fact]
    public async Task HandleAsync_SortByAssetClassNameAscending()
    {
        // Arrange
        await using var context = TestContextFactory.Create();
        TestData.SeedReferenceData(context);
        context.AssetClasses.AddRange(
            TestData.AssetClass(id: 2, name: "Bonds"),
            TestData.AssetClass(id: 3, name: "Equities"));
        context.AssetSubclasses.AddRange(
            TestData.AssetSubclass(id: 2, name: "Gilts", assetClassId: 2),
            TestData.AssetSubclass(id: 3, name: "UK Equities", assetClassId: 3));
        context.Instruments.AddRange(
            new Instrument { Id = 1, Name = "Alpha Inc", Ticker = "ALP", CurrencyId = "GBP", InstrumentTypeId = 1, AssetSubclassId = 3 },
            new Instrument { Id = 2, Name = "Zebra Gilt", Ticker = "ZEB", CurrencyId = "GBP", InstrumentTypeId = 1, AssetSubclassId = 2 }
        );
        await context.SaveChangesAsync();

        var handler = new GetInstrumentsHandler(context);

        // Act
        var result = await handler.HandleAsync(new GetInstrumentsQuery { SortBy = "assetClassName", SortDirection = "asc" });

        // Assert
        Assert.Equal(["Bonds", "Equities"], result.Items.Select(i => i.AssetClassName));
    }

    [Fact]
    public async Task HandleAsync_EmptyDatabase_ReturnsEmptyPagedResult()
    {
        await using var context = TestContextFactory.Create();
        var handler = new GetInstrumentsHandler(context);

        var result = await handler.HandleAsync(new GetInstrumentsQuery());

        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Items);
    }

}
