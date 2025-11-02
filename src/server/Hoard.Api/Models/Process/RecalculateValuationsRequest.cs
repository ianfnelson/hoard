namespace Hoard.Api.Models.Process;

public class RecalculateValuationsRequest
{
    public DateOnly AsOfDate { get; } = DateOnly.FromDateTime(DateTime.Now);
}