using Hoard.Messages.Valuations;

namespace Hoard.Core.Application.Valuations;

public record TriggerBackfillValuationsCommand(Guid CorrelationId, DateOnly? StartDate, DateOnly? EndDate)
    : ITriggerCommand
{
    public object ToBusCommand() => new StartBackfillValuationsSagaCommand(CorrelationId, StartDate, EndDate);
}