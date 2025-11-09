using Hoard.Messages.Valuations;

namespace Hoard.Api.Models.Valuations;

public class CalculateValuationsRequest
{
    public DateOnly? AsOfDate { get; set; }

    public StartCalculateValuationsSagaCommand ToCommand()
    {
        return new StartCalculateValuationsSagaCommand(Guid.NewGuid()) { AsOfDate = AsOfDate };
    }
}