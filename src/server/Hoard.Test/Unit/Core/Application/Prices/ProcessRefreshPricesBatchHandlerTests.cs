using System.Reflection;
using Hoard.Core.Application.Prices;
using Hoard.Core.Domain.Entities;
using Hoard.Core.Infrastructure.Integrations;

namespace Hoard.Test.Unit.Core.Application.Prices;

// The handler loads InstrumentDataFeed with ToJson() owned Configuration,
// which InMemory provider doesn't round-trip. We test the private static
// business logic methods via reflection instead.

public class ProcessRefreshPricesBatchHandlerTests
{
    private static bool InvokeHasAnyFieldChanged(Price price, PriceDto dto)
    {
        var method = typeof(ProcessRefreshPricesBatchHandler)
            .GetMethod("HasAnyFieldChanged", BindingFlags.NonPublic | BindingFlags.Static)!;
        return (bool)method.Invoke(null, [price, dto])!;
    }

    private static bool InvokeHasNotableChange(Price price, PriceDto dto)
    {
        var method = typeof(ProcessRefreshPricesBatchHandler)
            .GetMethod("HasNotableChange", BindingFlags.NonPublic | BindingFlags.Static)!;
        return (bool)method.Invoke(null, [price, dto])!;
    }

    [Fact]
    public void HasAnyFieldChanged_AllFieldsSame_ReturnsFalse()
    {
        var price = new Price
        {
            Open = 100m, High = 105m, Low = 95m, Close = 102m,
            Volume = 1000, AdjustedClose = 102m, Source = "Yahoo",
            InstrumentId = 1, AsOfDate = new DateOnly(2024, 1, 15), RetrievedUtc = DateTime.UtcNow
        };
        var dto = new PriceDto
        {
            Open = 100m, High = 105m, Low = 95m, Close = 102m,
            Volume = 1000, AdjustedClose = 102m, Source = "Yahoo",
            Date = new DateOnly(2024, 1, 15)
        };

        Assert.False(InvokeHasAnyFieldChanged(price, dto));
    }

    [Fact]
    public void HasAnyFieldChanged_CloseDiffers_ReturnsTrue()
    {
        var price = new Price
        {
            Open = 100m, High = 105m, Low = 95m, Close = 102m,
            Volume = 1000, AdjustedClose = 102m, Source = "Yahoo",
            InstrumentId = 1, AsOfDate = new DateOnly(2024, 1, 15), RetrievedUtc = DateTime.UtcNow
        };
        var dto = new PriceDto
        {
            Open = 100m, High = 105m, Low = 95m, Close = 103m,
            Volume = 1000, AdjustedClose = 102m, Source = "Yahoo",
            Date = new DateOnly(2024, 1, 15)
        };

        Assert.True(InvokeHasAnyFieldChanged(price, dto));
    }

    [Fact]
    public void HasAnyFieldChanged_VolumeDiffers_ReturnsTrue()
    {
        var price = new Price
        {
            Open = 100m, High = 105m, Low = 95m, Close = 102m,
            Volume = 1000, AdjustedClose = 102m, Source = "Yahoo",
            InstrumentId = 1, AsOfDate = new DateOnly(2024, 1, 15), RetrievedUtc = DateTime.UtcNow
        };
        var dto = new PriceDto
        {
            Open = 100m, High = 105m, Low = 95m, Close = 102m,
            Volume = 2000, AdjustedClose = 102m, Source = "Yahoo",
            Date = new DateOnly(2024, 1, 15)
        };

        Assert.True(InvokeHasAnyFieldChanged(price, dto));
    }

    [Fact]
    public void HasNotableChange_CloseChanged_ReturnsTrue()
    {
        var price = new Price
        {
            Close = 100m, InstrumentId = 1,
            AsOfDate = new DateOnly(2024, 1, 15), Source = "Yahoo", RetrievedUtc = DateTime.UtcNow
        };
        var dto = new PriceDto { Close = 105m, Source = "Yahoo", Date = new DateOnly(2024, 1, 15) };

        Assert.True(InvokeHasNotableChange(price, dto));
    }

    [Fact]
    public void HasNotableChange_CloseUnchanged_ReturnsFalse()
    {
        var price = new Price
        {
            Close = 100m, InstrumentId = 1,
            AsOfDate = new DateOnly(2024, 1, 15), Source = "Yahoo", RetrievedUtc = DateTime.UtcNow
        };
        var dto = new PriceDto { Close = 100m, Source = "Yahoo", Date = new DateOnly(2024, 1, 15) };

        Assert.False(InvokeHasNotableChange(price, dto));
    }
}
