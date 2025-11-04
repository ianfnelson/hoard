using Hoard.Core.Messages.Valuations;

namespace Hoard.Api.Models.Valuations;

public class CalculateValuationsRequest
{
    public DateOnly? AsOfDate { get; set; }

    public CalculateValuationsCommand ToCommand()
    {
        return new CalculateValuationsCommand { AsOfDate = AsOfDate };
    }
}