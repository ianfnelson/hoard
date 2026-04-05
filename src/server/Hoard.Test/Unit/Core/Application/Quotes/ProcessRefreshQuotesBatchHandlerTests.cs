using System.Reflection;
using Hoard.Core.Application.Quotes;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Core.Infrastructure.Integrations;
using Hoard.Messages.Quotes;
using Hoard.Test.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Rebus.Bus;

namespace Hoard.Test.Unit.Core.Application.Quotes;

// The handler's Include(InstrumentType) combined with ToJson() owned Configuration
// causes InMemory provider to drop the Configuration. We test HandleAsync guard
// paths (no DataFeedConfiguration needed) and business logic via reflection.

public class ProcessRefreshQuotesBatchHandlerTests : IDisposable
{
    private readonly HoardContext _context;
    private readonly Mock<IBus> _bus = new();
    private readonly Mock<IQuoteService> _quoteService = new();
    private readonly ProcessRefreshQuotesBatchHandler _handler;

    public ProcessRefreshQuotesBatchHandlerTests()
    {
        _context = TestContextFactory.Create();
        _handler = new ProcessRefreshQuotesBatchHandler(
            _bus.Object,
            _context,
            NullLogger<ProcessRefreshQuotesBatchHandler>.Instance,
            _quoteService.Object);
    }

    #region HandleAsync guard paths

    [Fact]
    public async Task HandleAsync_EmptyInstrumentIds_PublishesBatchRefreshedEvent()
    {
        var runId = Guid.NewGuid();

        await _handler.HandleAsync(new ProcessRefreshQuotesBatchCommand(runId, 0, []));

        _quoteService.Verify(s => s.GetQuotesAsync(It.IsAny<IEnumerable<string>>()), Times.Never);
        _bus.Verify(b => b.Publish(It.Is<QuoteBatchRefreshedEvent>(e =>
            e.QuotesRunId == runId && e.BatchIndex == 0), null), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_NoInstrumentsFound_PublishesBatchRefreshedEvent()
    {
        var runId = Guid.NewGuid();

        await _handler.HandleAsync(new ProcessRefreshQuotesBatchCommand(runId, 0, [999]));

        _quoteService.Verify(s => s.GetQuotesAsync(It.IsAny<IEnumerable<string>>()), Times.Never);
        _bus.Verify(b => b.Publish(It.Is<QuoteBatchRefreshedEvent>(e =>
            e.QuotesRunId == runId), null), Times.Once);
    }

    #endregion

    #region UpsertQuotes

    private void InvokeUpsertQuotes(
        IEnumerable<Instrument> instruments,
        IReadOnlyDictionary<string, QuoteDto> freshQuotes,
        DateTime now,
        ICollection<Instrument> changed)
    {
        var method = typeof(ProcessRefreshQuotesBatchHandler)
            .GetMethod("UpsertQuotes", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Method 'UpsertQuotes' not found — was it renamed?");
        method.Invoke(_handler, [instruments, freshQuotes, now, changed]);
    }

    [Fact]
    public void UpsertQuotes_NewQuote_CreatesQuoteAndMarksChanged()
    {
        var instrument = new Instrument
        {
            Id = 1, Name = "Test", Ticker = "TST", CurrencyId = "GBP",
            InstrumentTypeId = 1, AssetSubclassId = 1
        };
        instrument.DataFeeds.Add(new InstrumentDataFeed
        {
            InstrumentId = 1, DataFeedId = DataFeed.Quotes,
            Configuration = new DataFeedConfiguration { Ticker = "TST" },
            CreatedUtc = DateTime.UtcNow
        });

        var freshQuotes = new Dictionary<string, QuoteDto>
        {
            ["TST"] = new()
            {
                Symbol = "TST", Name = "Test", Source = "Yahoo",
                RegularMarketPrice = 150m, Bid = 149m, Ask = 151m,
                FiftyTwoWeekHigh = 200m, FiftyTwoWeekLow = 80m,
                RegularMarketChange = 5m, RegularMarketChangePercent = 3.4m
            }
        };

        var changed = new List<Instrument>();
        var now = DateTime.UtcNow;
        InvokeUpsertQuotes([instrument], freshQuotes, now, changed);

        Assert.Single(changed);
        Assert.NotNull(instrument.Quote);
        Assert.Equal(150m, instrument.Quote!.RegularMarketPrice);
        Assert.Equal(149m, instrument.Quote.Bid);
        Assert.Equal(151m, instrument.Quote.Ask);
        Assert.Equal(200m, instrument.Quote.FiftyTwoWeekHigh);
        Assert.Equal(80m, instrument.Quote.FiftyTwoWeekLow);
        Assert.Equal(now, instrument.Quote.RetrievedUtc);
    }

    [Fact]
    public void UpsertQuotes_ExistingQuoteWithNotableChange_UpdatesAndMarksChanged()
    {
        var instrument = new Instrument
        {
            Id = 1, Name = "Test", Ticker = "TST", CurrencyId = "GBP",
            InstrumentTypeId = 1, AssetSubclassId = 1,
            Quote = new Quote
            {
                InstrumentId = 1, Source = "Yahoo", RegularMarketPrice = 100m,
                Bid = 99m, Ask = 101m, RetrievedUtc = DateTime.UtcNow
            }
        };
        instrument.DataFeeds.Add(new InstrumentDataFeed
        {
            InstrumentId = 1, DataFeedId = DataFeed.Quotes,
            Configuration = new DataFeedConfiguration { Ticker = "TST" },
            CreatedUtc = DateTime.UtcNow
        });

        var freshQuotes = new Dictionary<string, QuoteDto>
        {
            ["TST"] = new()
            {
                Symbol = "TST", Name = "Test", Source = "Yahoo",
                RegularMarketPrice = 110m, Bid = 109m, Ask = 111m,
                RegularMarketChange = 10m, RegularMarketChangePercent = 10m
            }
        };

        var changed = new List<Instrument>();
        InvokeUpsertQuotes([instrument], freshQuotes, DateTime.UtcNow, changed);

        Assert.Single(changed);
        Assert.Equal(110m, instrument.Quote!.RegularMarketPrice);
        Assert.Equal(109m, instrument.Quote.Bid);
    }

    [Fact]
    public void UpsertQuotes_ExistingQuoteNoNotableChange_UpdatesButDoesNotMarkChanged()
    {
        var instrument = new Instrument
        {
            Id = 1, Name = "Test", Ticker = "TST", CurrencyId = "GBP",
            InstrumentTypeId = 1, AssetSubclassId = 1,
            Quote = new Quote
            {
                InstrumentId = 1, Source = "Yahoo", RegularMarketPrice = 100m,
                Bid = 99m, Ask = 101m, RetrievedUtc = DateTime.UtcNow
            }
        };
        instrument.DataFeeds.Add(new InstrumentDataFeed
        {
            InstrumentId = 1, DataFeedId = DataFeed.Quotes,
            Configuration = new DataFeedConfiguration { Ticker = "TST" },
            CreatedUtc = DateTime.UtcNow
        });

        var freshQuotes = new Dictionary<string, QuoteDto>
        {
            ["TST"] = new()
            {
                Symbol = "TST", Name = "Test", Source = "Yahoo",
                RegularMarketPrice = 100m, Bid = 98m, Ask = 102m,
                RegularMarketChange = 0m, RegularMarketChangePercent = 0m
            }
        };

        var changed = new List<Instrument>();
        InvokeUpsertQuotes([instrument], freshQuotes, DateTime.UtcNow, changed);

        Assert.Empty(changed);
        Assert.Equal(98m, instrument.Quote!.Bid);
        Assert.Equal(102m, instrument.Quote.Ask);
    }

    [Fact]
    public void UpsertQuotes_NoQuoteFeedOnInstrument_SkipsIt()
    {
        var instrument = TestData.Instrument();

        var freshQuotes = new Dictionary<string, QuoteDto>
        {
            ["TST"] = new()
            {
                Symbol = "TST", Name = "Test", Source = "Yahoo",
                RegularMarketPrice = 100m
            }
        };

        var changed = new List<Instrument>();
        InvokeUpsertQuotes([instrument], freshQuotes, DateTime.UtcNow, changed);

        Assert.Empty(changed);
        Assert.Null(instrument.Quote);
    }

    [Fact]
    public void UpsertQuotes_TickerNotInResponse_SkipsIt()
    {
        var instrument = TestData.Instrument();
        instrument.DataFeeds.Add(new InstrumentDataFeed
        {
            InstrumentId = 1, DataFeedId = DataFeed.Quotes,
            Configuration = new DataFeedConfiguration { Ticker = "TST" },
            CreatedUtc = DateTime.UtcNow
        });

        var freshQuotes = new Dictionary<string, QuoteDto>();

        var changed = new List<Instrument>();
        InvokeUpsertQuotes([instrument], freshQuotes, DateTime.UtcNow, changed);

        Assert.Empty(changed);
        Assert.Null(instrument.Quote);
    }

    [Fact]
    public void UpsertQuotes_MultipleInstruments_ProcessesAll()
    {
        var inst1 = new Instrument
        {
            Id = 1, Name = "Stock A", Ticker = "AAA", CurrencyId = "GBP",
            InstrumentTypeId = 1, AssetSubclassId = 1
        };
        inst1.DataFeeds.Add(new InstrumentDataFeed
        {
            InstrumentId = 1, DataFeedId = DataFeed.Quotes,
            Configuration = new DataFeedConfiguration { Ticker = "AAA" },
            CreatedUtc = DateTime.UtcNow
        });

        var inst2 = new Instrument
        {
            Id = 2, Name = "Stock B", Ticker = "BBB", CurrencyId = "GBP",
            InstrumentTypeId = 1, AssetSubclassId = 1
        };
        inst2.DataFeeds.Add(new InstrumentDataFeed
        {
            InstrumentId = 2, DataFeedId = DataFeed.Quotes,
            Configuration = new DataFeedConfiguration { Ticker = "BBB" },
            CreatedUtc = DateTime.UtcNow
        });

        var freshQuotes = new Dictionary<string, QuoteDto>
        {
            ["AAA"] = new() { Symbol = "AAA", Name = "A", Source = "Yahoo", RegularMarketPrice = 50m },
            ["BBB"] = new() { Symbol = "BBB", Name = "B", Source = "Yahoo", RegularMarketPrice = 75m }
        };

        var changed = new List<Instrument>();
        InvokeUpsertQuotes([inst1, inst2], freshQuotes, DateTime.UtcNow, changed);

        Assert.Equal(2, changed.Count);
        Assert.Equal(50m, inst1.Quote!.RegularMarketPrice);
        Assert.Equal(75m, inst2.Quote!.RegularMarketPrice);
    }

    #endregion

    #region Static helper tests

    private static bool InvokeHasNotableChange(Quote quote, QuoteDto dto)
    {
        var method = typeof(ProcessRefreshQuotesBatchHandler)
            .GetMethod("HasNotableChange", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("Method 'HasNotableChange' not found — was it renamed?");
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

    #endregion

    public void Dispose()
    {
        _context.Dispose();
    }
}
