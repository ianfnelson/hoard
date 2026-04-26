using Hoard.Core.Application.Instruments;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Test.Shared;
using Microsoft.Extensions.Logging.Abstractions;

namespace Hoard.Test.Unit.Core.Application.Instruments;

public class GetPriceSeriesHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsCandlesForDateRange()
    {
        // Arrange
        await using var context = TestContextFactory.Create();
        context.Instruments.Add(TestData.Instrument());
        context.Prices.AddRange(
            CreateOhlcPrice(1, new DateOnly(2024, 1, 1), 100m, 105m, 98m, 103m),
            CreateOhlcPrice(2, new DateOnly(2024, 1, 2), 103m, 107m, 101m, 106m),
            CreateOhlcPrice(3, new DateOnly(2024, 1, 3), 106m, 110m, 104m, 108m)
        );
        await context.SaveChangesAsync();

        var handler = new GetPriceSeriesHandler(context, NullLogger<GetPriceSeriesHandler>.Instance, TimeProvider.System);
        var query = new GetPriceSeriesQuery(1)
        {
            From = new DateOnly(2024, 1, 1),
            To = new DateOnly(2024, 1, 2)
        };

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Candles.Count);
    }

    [Fact]
    public async Task HandleAsync_ExcludesPricesWithMissingOhlc()
    {
        // Arrange
        await using var context = TestContextFactory.Create();
        context.Instruments.Add(TestData.Instrument());
        context.Prices.AddRange(
            CreateOhlcPrice(1, new DateOnly(2024, 1, 1), 100m, 105m, 98m, 103m),
            new Price
            {
                Id = 2, InstrumentId = 1, AsOfDate = new DateOnly(2024, 1, 2),
                Close = 106m, AdjustedClose = 106m, Source = "Test",
                RetrievedUtc = DateTime.UtcNow
                // No Open/High/Low — should be excluded
            }
        );
        await context.SaveChangesAsync();

        var handler = new GetPriceSeriesHandler(context, NullLogger<GetPriceSeriesHandler>.Instance, TimeProvider.System);
        var query = new GetPriceSeriesQuery(1)
        {
            From = new DateOnly(2024, 1, 1),
            To = new DateOnly(2024, 1, 3)
        };

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Candles);
    }

    [Fact]
    public async Task HandleAsync_NonExistentInstrument_ReturnsNull()
    {
        await using var context = TestContextFactory.Create();
        var handler = new GetPriceSeriesHandler(context, NullLogger<GetPriceSeriesHandler>.Instance, TimeProvider.System);

        var result = await handler.HandleAsync(new GetPriceSeriesQuery(999));

        Assert.Null(result);
    }

    private static Price CreateOhlcPrice(int id, DateOnly date, decimal open, decimal high, decimal low, decimal close)
        => new()
        {
            Id = id, InstrumentId = 1, AsOfDate = date,
            Open = open, High = high, Low = low, Close = close,
            AdjustedClose = close, Source = "Test", RetrievedUtc = DateTime.UtcNow
        };
}
