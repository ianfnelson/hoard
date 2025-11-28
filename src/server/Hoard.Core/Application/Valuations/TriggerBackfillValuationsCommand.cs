using Hoard.Messages;
using Hoard.Messages.Valuations;

namespace Hoard.Core.Application.Valuations;

public record TriggerBackfillValuationsCommand(Guid CorrelationId, PipelineMode PipelineMode, int? InstrumentId, DateOnly? StartDate, DateOnly? EndDate)
    : ITriggerCommand
{
    public object ToBusCommand() => new StartBackfillValuationsSagaCommand(CorrelationId, PipelineMode, InstrumentId, StartDate, EndDate);
}