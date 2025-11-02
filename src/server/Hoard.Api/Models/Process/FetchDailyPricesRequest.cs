namespace Hoard.Api.Models.Process;

public class FetchDailyPricesRequest
{
    public DateOnly AsOfDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
}