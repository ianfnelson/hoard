namespace Hoard.Api.Models.Process;

public class FetchDailyPricesRequest
{
    public DateOnly AsOfDate { get; } = DateOnly.FromDateTime(DateTime.Now);
}