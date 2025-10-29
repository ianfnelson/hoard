namespace Hoard.Core.Services;

public interface IQuoteService
{
    Task<IReadOnlyList<Quote>> GetQuotesAsync(IEnumerable<string> tickers);
}

