using Hoard.Core.Application.Holdings;

namespace Hoard.Api.Models.Holdings;

public class BackfillHoldingsRequest
{
    public DateOnly? StartDate { get; init; }
    public DateOnly? EndDate { get; init; }

    public TriggerBackfillHoldingsCommand ToCommand()
    {
        return new TriggerBackfillHoldingsCommand(Guid.NewGuid(), StartDate, EndDate);
    }
}