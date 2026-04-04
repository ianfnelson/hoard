using System.Reflection;
using Hoard.Core.Application.Quotes;
using Hoard.Core.Domain.Entities;
using Hoard.Core.Infrastructure.Integrations;

namespace Hoard.Test.Unit.Core.Application.Quotes;

// The handler loads InstrumentDataFeed with ToJson() owned Configuration,
// which InMemory provider doesn't round-trip. We test the private static
// business logic methods via reflection instead.

public class ProcessRefreshQuotesBatchHandlerTests
{
    private static bool InvokeHasNotableChange(Quote quote, QuoteDto dto)
    {
        var method = typeof(ProcessRefreshQuotesBatchHandler)
            .GetMethod("HasNotableChange", BindingFlags.NonPublic | BindingFlags.Static)!;
        return (bool)method.Invoke(null, [quote, dto])!;
    }

    [Fact]
    public void HasNotableChange_PriceChanged_ReturnsTrue()
    {
        var quote = new Quote { RegularMarketPrice = 100m, InstrumentId = 1, Source = "Yahoo" };
        var dto = new QuoteDto { RegularMarketPrice = 105m, Symbol = "TST", Name = "Test", Source = "Yahoo" };

        Assert.True(InvokeHasNotableChange(quote, dto));
    }

    [Fact]
    public void HasNotableChange_PriceUnchanged_ReturnsFalse()
    {
        var quote = new Quote { RegularMarketPrice = 100m, InstrumentId = 1, Source = "Yahoo" };
        var dto = new QuoteDto { RegularMarketPrice = 100m, Symbol = "TST", Name = "Test", Source = "Yahoo" };

        Assert.False(InvokeHasNotableChange(quote, dto));
    }

    [Fact]
    public void HasNotableChange_OtherFieldsChanged_ReturnsFalse()
    {
        var quote = new Quote { RegularMarketPrice = 100m, Bid = 99m, Ask = 101m, InstrumentId = 1, Source = "Yahoo" };
        var dto = new QuoteDto { RegularMarketPrice = 100m, Bid = 98m, Ask = 102m, Symbol = "TST", Name = "Test", Source = "Yahoo" };

        Assert.False(InvokeHasNotableChange(quote, dto));
    }
}
