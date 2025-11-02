namespace Hoard.Api.Models.Process;

public class BackfillHoldingsRequest
{
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    
}