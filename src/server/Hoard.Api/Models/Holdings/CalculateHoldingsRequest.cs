using Hoard.Core.Application.Holdings;

namespace Hoard.Api.Models.Holdings;

public class CalculateHoldingsRequest
{
    public DateOnly? AsOfDate { get; set; }

    public TriggerCalculateHoldingsCommand ToCommand()
    {
        return new TriggerCalculateHoldingsCommand(Guid.NewGuid(), AsOfDate);
    }
}