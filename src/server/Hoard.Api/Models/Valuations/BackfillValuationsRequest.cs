using Hoard.Messages.Valuations;

namespace Hoard.Api.Models.Valuations;

public class BackfillValuationsRequest
{
    public DateOnly? StartDate { get; init; }
    
    public DateOnly? EndDate { get; init; }
    
    public StartBackfillValuationsSagaCommand ToCommand()
    {
        return new StartBackfillValuationsSagaCommand(Guid.NewGuid())
        {
            StartDate = StartDate,
            EndDate = EndDate
        };
    }
}