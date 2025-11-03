namespace Hoard.Core.Services;

public interface IHistoricalPriceService
{
    Task<IReadOnlyList<HistoricalPriceDto>> GetHistoricalAsync(string ticker, DateOnly from, DateOnly to);
}