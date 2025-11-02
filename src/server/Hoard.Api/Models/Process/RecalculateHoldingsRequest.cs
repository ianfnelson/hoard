namespace Hoard.Api.Models.Process;

public class RecalculateHoldingsRequest
{
    public DateOnly AsOfDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
}