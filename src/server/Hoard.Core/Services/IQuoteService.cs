namespace Hoard.Core.Services;

public interface IQuoteService
{
    Task<Dictionary<string, QuoteDto>> GetQuotesAsync(IEnumerable<string> tickers);
}

