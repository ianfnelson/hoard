using Hoard.Messages.Holdings;

namespace Hoard.Api.Models.Holdings;

public class BackfillHoldingsRequest
{
    public DateOnly? StartDate { get; init; }
    public DateOnly? EndDate { get; init; }

    public StartBackfillHoldingsSagaCommand ToCommand()
    {
        return new StartBackfillHoldingsSagaCommand(Guid.NewGuid())
        {
            StartDate = StartDate,
            EndDate = EndDate
        };
    }
}