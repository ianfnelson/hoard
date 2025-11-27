using Hoard.Core.Application.Prices;
using Hoard.Messages;

namespace Hoard.Api.Models.Prices;

public class BackfillPricesRequest
{
    public int? InstrumentId { get; init; }
    public DateOnly? StartDate { get; init; }
    public DateOnly? EndDate { get; init; }

    public TriggerRefreshPricesCommand ToCommand()
    {
        return new TriggerRefreshPricesCommand(
            Guid.NewGuid(), PipelineMode.Backfill, InstrumentId, StartDate, EndDate);
    }
}