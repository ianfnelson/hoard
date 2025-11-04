using Hoard.Core.Messages.Holdings;

namespace Hoard.Api.Models.Holdings;

public class BackfillHoldingsRequest
{
    public DateOnly? StartDate { get; init; }
    public DateOnly? EndDate { get; init; }

    public BackfillHoldingsCommand ToCommand()
    {
        return new BackfillHoldingsCommand(Guid.NewGuid())
        {
            StartDate = StartDate,
            EndDate = EndDate
        };
    }
}