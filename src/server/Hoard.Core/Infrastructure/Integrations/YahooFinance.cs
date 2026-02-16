using YahooFinanceApi;

namespace Hoard.Core.Infrastructure.Integrations;

public interface IPriceService
{
    Task<IReadOnlyList<PriceDto>> GetPricesAsync(string ticker, DateOnly from, DateOnly to, CancellationToken ct = default);
}

public class PriceDto
{
    public DateOnly Date { get; set; }
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public long Volume { get; set; }
    public decimal AdjustedClose { get; set; }
    public required string Source { get; set; }
}

public interface IQuoteService
{
    Task<Dictionary<string, QuoteDto>> GetQuotesAsync(IEnumerable<string> tickers);
}

public class QuoteDto
{
    public required string Symbol { get; set; }
    public required string Name { get; set; }
    public decimal Bid { get; set; }
    public decimal Ask { get; set; }
    public decimal FiftyTwoWeekHigh { get; set; }
    public decimal FiftyTwoWeekLow { get; set; }
    public decimal RegularMarketPrice { get; set; }
    public decimal RegularMarketChange { get; set; }
    public decimal RegularMarketChangePercent { get; set; }
    public required string Source { get; set; }
}

public class YahooFinance
    : IPriceService, IQuoteService
{
    private const string Source = "Yahoo! Finance";

    private static readonly Field[] Fields =
    [
        Field.Symbol, Field.LongName, Field.Bid, Field.Ask, Field.FiftyTwoWeekHigh, Field.FiftyTwoWeekLow,
        Field.RegularMarketPrice, Field.RegularMarketChange, Field.RegularMarketChangePercent
    ];

    public async Task<Dictionary<string, QuoteDto>> GetQuotesAsync(IEnumerable<string> tickers)
    {
        var tickerList = tickers.ToList();

        var securities = await Yahoo.Symbols(tickerList.ToArray())
            .Fields(Fields)
            .QueryAsync();

        var quotes = securities
            .Select(x => MapSecurityToQuote(x.Value))
            .ToDictionary(x => x.Symbol);

        return quotes;
    }

    public async Task<IReadOnlyList<PriceDto>> GetPricesAsync(string ticker, DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        Yahoo.IgnoreEmptyRows = true;
        var history = await Yahoo.GetHistoricalAsync(ticker, GetDateTime(from), GetDateTime(to), token:ct);

        var results = history
            .Select(MapCandleToHistoricalPrice)
            .ToList();

        return results;
    }

    private static QuoteDto MapSecurityToQuote(Security security)
    {
        return new QuoteDto
        {
            Symbol = security.Symbol,
            Ask = GetRoundedDecimal(security.Ask),
            Bid = GetRoundedDecimal(security.Bid),
            Name = security.LongName,
            FiftyTwoWeekHigh = GetRoundedDecimal(security.FiftyTwoWeekHigh),
            FiftyTwoWeekLow = GetRoundedDecimal(security.FiftyTwoWeekLow),
            RegularMarketPrice = GetRoundedDecimal(security.RegularMarketPrice),
            RegularMarketChange = GetRoundedDecimal(security.RegularMarketChange),
            RegularMarketChangePercent = GetRoundedDecimal(security.RegularMarketChangePercent),
            Source = Source
        };
    }

    private static PriceDto MapCandleToHistoricalPrice(Candle candle)
    {
        return new PriceDto
        {
            Date = GetDateOnly(candle.DateTime),
            Open = GetRoundedDecimal(candle.Open),
            High = GetRoundedDecimal(candle.High),
            Low = GetRoundedDecimal(candle.Low),
            Close = GetRoundedDecimal(candle.Close),
            Volume = candle.Volume,
            AdjustedClose = GetRoundedDecimal(candle.AdjustedClose),
            Source = Source
        };
    }

    private static DateTime GetDateTime(DateOnly date) => new(date.Year, date.Month, date.Day);
    private static DateOnly GetDateOnly(DateTime dateTime) => new(dateTime.Year, dateTime.Month, dateTime.Day);

    private static decimal GetRoundedDecimal(double value) =>
        GetRoundedDecimal((decimal)value);

    private static decimal GetRoundedDecimal(decimal value) =>
        Math.Round(value, 4, MidpointRounding.AwayFromZero);
}