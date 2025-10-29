namespace Hoard.Core.Services;

public interface IHistoricalPriceService
{
    Task<IReadOnlyList<HistoricalPrice>> GetHistoricalAsync(string ticker, DateOnly from, DateOnly to);
}