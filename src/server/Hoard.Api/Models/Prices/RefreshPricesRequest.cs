using Hoard.Core.Application.Prices;

namespace Hoard.Api.Models.Prices;

public class RefreshPricesRequest
{
    public DateOnly? AsOfDate { get; set; }
    public int? InstrumentId { get; set; }

    public TriggerRefreshPricesCommand ToCommand()
    {
        return new TriggerRefreshPricesCommand( Guid.NewGuid(), InstrumentId, AsOfDate);
    }
}