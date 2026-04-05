using Hoard.Core.Application.Instruments;
using Hoard.Core.Data;
using Hoard.Test.Shared;
using Microsoft.Extensions.Logging.Abstractions;

namespace Hoard.Test.Unit.Core.Application.Instruments;

public class GetPricesHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsPagedPricesSortedByDateDescending()
    {
        // Arrange
        await using var context = TestContextFactory.Create();
        context.Instruments.Add(TestData.Instrument());
        context.Prices.AddRange(
            TestData.Price(asOfDate: new DateOnly(2024, 1, 1), close: 100m),
            TestData.Price(asOfDate: new DateOnly(2024, 1, 3), close: 102m),
            TestData.Price(asOfDate: new DateOnly(2024, 1, 2), close: 101m)
        );
        await context.SaveChangesAsync();

        var handler = new GetPricesHandler(context, NullLogger<GetPricesHandler>.Instance);

        // Act
        var result = await handler.HandleAsync(new GetPricesQuery(1) { PageSize = 2 });

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(new DateOnly(2024, 1, 3), result.Items[0].AsOfDate);
        Assert.Equal(new DateOnly(2024, 1, 2), result.Items[1].AsOfDate);
    }

    [Fact]
    public async Task HandleAsync_NonExistentInstrument_ReturnsNull()
    {
        await using var context = TestContextFactory.Create();
        var handler = new GetPricesHandler(context, NullLogger<GetPricesHandler>.Instance);

        var result = await handler.HandleAsync(new GetPricesQuery(999));

        Assert.Null(result);
    }
}
