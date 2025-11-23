using Hoard.Messages.Valuations;

namespace Hoard.Core.Application.Valuations;

public record TriggerBackfillValuationsCommand(Guid CorrelationId, int? InstrumentId, DateOnly? StartDate, DateOnly? EndDate)
    : ITriggerCommand
{
    public object ToBusCommand() => new StartBackfillValuationsSagaCommand(CorrelationId, InstrumentId, StartDate, EndDate);
}