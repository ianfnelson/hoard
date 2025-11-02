namespace Hoard.Api.Models.Process;

public class RecalculateHoldingsRequest
{
    public DateOnly AsOfDate { get; } = DateOnly.FromDateTime(DateTime.Now);
}