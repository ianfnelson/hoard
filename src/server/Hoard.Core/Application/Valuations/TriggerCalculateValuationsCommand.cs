using Hoard.Messages.Valuations;

namespace Hoard.Core.Application.Valuations;

public record TriggerCalculateValuationsCommand(Guid CorrelationId, DateOnly? AsOfDate) : 
    ITriggerCommand
{
    public object ToBusCommand() => new StartCalculateValuationsSagaCommand(CorrelationId, null, AsOfDate);
}
