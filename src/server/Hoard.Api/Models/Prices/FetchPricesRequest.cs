using Hoard.Core.Messages.Prices;

namespace Hoard.Api.Models.Prices;

public class FetchPricesRequest
{
    public DateOnly? AsOfDate { get; set; }

    public FetchPricesCommand ToCommand()
    {
        return new FetchPricesCommand { AsOfDate = AsOfDate };
    }
}