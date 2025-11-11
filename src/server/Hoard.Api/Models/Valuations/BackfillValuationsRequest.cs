using Hoard.Core.Application.Valuations;

namespace Hoard.Api.Models.Valuations;

public class BackfillValuationsRequest
{
    public DateOnly? StartDate { get; init; }
    
    public DateOnly? EndDate { get; init; }
    
    public TriggerBackfillValuationsCommand ToCommand()
    {
        return new TriggerBackfillValuationsCommand(Guid.NewGuid(), StartDate, EndDate);
    }
}