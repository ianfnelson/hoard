namespace Hoard.Api.Models.Process;

public class RecalculateValuationsRequest
{
    public DateOnly AsOfDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
}