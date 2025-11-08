using Hoard.Messages.Prices;

namespace Hoard.Api.Models.Prices;

public class RefreshPricesRequest
{
    public DateOnly? AsOfDate { get; set; }

    public RefreshPricesBusCommand ToCommand()
    {
        return new RefreshPricesBusCommand(Guid.NewGuid())
        {
            AsOfDate = AsOfDate
        };
    }
}