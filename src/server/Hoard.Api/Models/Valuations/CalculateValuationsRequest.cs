using Hoard.Core.Application.Valuations;

namespace Hoard.Api.Models.Valuations;

public class CalculateValuationsRequest
{
    public DateOnly? AsOfDate { get; set; }

    public TriggerCalculateValuationsCommand ToCommand()
    {
        return new TriggerCalculateValuationsCommand(Guid.NewGuid(), AsOfDate);
    }
}