using Hoard.Core.Messages.Holdings;

namespace Hoard.Api.Models.Holdings;

public class CalculateHoldingsRequest
{
    public DateOnly? AsOfDate { get; set; }

    public CalculateHoldingsCommand ToCommand()
    {
        return new CalculateHoldingsCommand
        {
            AsOfDate = AsOfDate
        };
    }
}