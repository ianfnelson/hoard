using Hoard.Core.Messages.Prices;

namespace Hoard.Api.Models.Prices;

public class RefreshPricesRequest
{
    public DateOnly? AsOfDate { get; set; }

    public RefreshPricesCommand ToCommand()
    {
        return new RefreshPricesCommand(Guid.NewGuid())
        {
            AsOfDate = AsOfDate
        };
    }
}