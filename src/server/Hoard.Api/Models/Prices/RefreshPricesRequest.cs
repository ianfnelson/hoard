using Hoard.Core.Application.Prices;

namespace Hoard.Api.Models.Prices;

public class RefreshPricesRequest
{
    public int? InstrumentId { get; init; }
    public DateOnly? StartDate { get; init; }
    public DateOnly? EndDate { get; init; }

    public TriggerRefreshPricesCommand ToCommand()
    {
        return new TriggerRefreshPricesCommand( Guid.NewGuid(), InstrumentId, StartDate, EndDate);
    }
}