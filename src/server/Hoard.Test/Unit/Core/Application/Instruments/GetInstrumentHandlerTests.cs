using Hoard.Core.Application.Instruments;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Test.Shared;
using Microsoft.Extensions.Logging.Abstractions;

namespace Hoard.Test.Unit.Core.Application.Instruments;

public class GetInstrumentHandlerTests
{
    [Fact]
    public async Task HandleAsync_ExistingInstrument_ReturnsDetail()
    {
        // Arrange
        await using var context = TestContextFactory.Create();
        SeedReferenceData(context);
        context.Instruments.Add(new Instrument
        {
            Id = 1, Name = "Fundsmith Equity", Ticker = "FUND", Isin = "GB123456",
            CurrencyId = "GBP", InstrumentTypeId = 1, AssetSubclassId = 1
        });
        await context.SaveChangesAsync();

        var handler = new GetInstrumentHandler(context, NullLogger<GetInstrumentHandler>.Instance);

        // Act
        var result = await handler.HandleAsync(new GetInstrumentQuery(1));

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Fundsmith Equity", result.Name);
        Assert.Equal("FUND", result.Ticker);
        Assert.Equal("GB123456", result.Isin);
        Assert.Equal("Stock", result.InstrumentTypeName);
        Assert.Equal("UK Equities", result.AssetSubclassName);
    }

    [Fact]
    public async Task HandleAsync_NonExistentInstrument_ReturnsNull()
    {
        await using var context = TestContextFactory.Create();
        var handler = new GetInstrumentHandler(context, NullLogger<GetInstrumentHandler>.Instance);

        var result = await handler.HandleAsync(new GetInstrumentQuery(999));

        Assert.Null(result);
    }

    private static void SeedReferenceData(HoardContext context)
    {
        context.InstrumentTypes.Add(new InstrumentType { Id = 1, Name = "Stock", Colour = "#FF0000" });
        context.Currencies.Add(new Currency { Id = "GBP", Name = "British Pound" });
        var assetClass = new AssetClass { Id = 1, Name = "Equities", Colour = "#0000FF" };
        assetClass.Subclasses.Add(new AssetSubclass { Id = 1, AssetClassId = 1, Name = "UK Equities", Colour = "#0000AA" });
        context.AssetClasses.Add(assetClass);
    }
}
