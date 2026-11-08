using Hoard.Core.Application.Instruments;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Test.Shared;

namespace Hoard.Test.Unit.Core.Application.Instruments;

public class GetInstrumentsLookupHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsInstrumentsOrderedByName_ExcludingCash()
    {
        // Arrange
        await using var context = TestContextFactory.Create();
        context.Instruments.AddRange(
            new Instrument { Id = 1, Name = "Zebra Corp", Ticker = "ZEB", CurrencyId = "GBP", InstrumentTypeId = 1 },
            new Instrument { Id = 2, Name = "Alpha Inc", Ticker = "ALP", CurrencyId = "GBP", InstrumentTypeId = 1 },
            new Instrument { Id = 3, Name = "Cash GBP", Ticker = "GBP", CurrencyId = "GBP", InstrumentTypeId = InstrumentType.Cash }
        );
        await context.SaveChangesAsync();

        var handler = new GetInstrumentsLookupHandler(context);

        // Act
        var result = await handler.HandleAsync(new GetInstrumentsLookupQuery(), CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Alpha Inc", result[0].Name);
        Assert.Equal("Zebra Corp", result[1].Name);
    }

    [Fact]
    public async Task HandleAsync_EmptyDatabase_ReturnsEmptyList()
    {
        await using var context = TestContextFactory.Create();
        var handler = new GetInstrumentsLookupHandler(context);

        var result = await handler.HandleAsync(new GetInstrumentsLookupQuery(), CancellationToken.None);

        Assert.Empty(result);
    }
}
