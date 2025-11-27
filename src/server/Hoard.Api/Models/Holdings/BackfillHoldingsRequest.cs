using Hoard.Core.Application.Holdings;
using Hoard.Messages;

namespace Hoard.Api.Models.Holdings;

public class BackfillHoldingsRequest
{
    public DateOnly? StartDate { get; init; }
    public DateOnly? EndDate { get; init; }

    public TriggerBackfillHoldingsCommand ToCommand()
    {
        return new TriggerBackfillHoldingsCommand(Guid.NewGuid(), PipelineMode.Backfill, StartDate, EndDate);
    }
}