using Hoard.Core.Application.Prices;

namespace Hoard.Api.Models.Prices;

public class BackfillPricesRequest
{
    public int? InstrumentId { get; init; }
    public DateOnly? StartDate { get; init; }
    public DateOnly? EndDate { get; init; }

    public TriggerBackfillPricesCommand ToCommand()
    {
        return new TriggerBackfillPricesCommand( Guid.NewGuid(), InstrumentId, StartDate, EndDate);
    }
}