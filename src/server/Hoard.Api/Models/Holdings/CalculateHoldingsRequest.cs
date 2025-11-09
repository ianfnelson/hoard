using Hoard.Messages.Holdings;

namespace Hoard.Api.Models.Holdings;

public class CalculateHoldingsRequest
{
    public DateOnly? AsOfDate { get; set; }

    public CalculateHoldingsBusCommand ToCommand()
    {
        return new CalculateHoldingsBusCommand(Guid.NewGuid())
        {
            AsOfDate = AsOfDate
        };
    }
}