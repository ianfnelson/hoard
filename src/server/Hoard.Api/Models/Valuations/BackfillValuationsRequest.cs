using Hoard.Core.Messages.Valuations;

namespace Hoard.Api.Models.Valuations;

public class BackfillValuationsRequest
{
    public DateOnly? StartDate { get; init; }
    
    public DateOnly? EndDate { get; init; }
    
    public BackfillValuationsCommand ToCommand()
    {
        return new BackfillValuationsCommand(Guid.NewGuid())
        {
            StartDate = StartDate,
            EndDate = EndDate
        };
    }
}