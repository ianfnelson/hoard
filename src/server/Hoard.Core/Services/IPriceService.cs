namespace Hoard.Core.Services;

public interface IPriceService
{
    Task<IReadOnlyList<PriceDto>> GetPricesAsync(string ticker, DateOnly from, DateOnly to);
}