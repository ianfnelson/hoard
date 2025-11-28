using Hoard.Messages;
using Hoard.Messages.Valuations;

namespace Hoard.Core.Application.Valuations;

public record TriggerBackfillValuationsCommand(Guid CorrelationId, int? InstrumentId, DateOnly? StartDate, DateOnly? EndDate, PipelineMode PipelineMode = PipelineMode.DaytimeReactive) 
    : ITriggerCommand
{
    public object ToBusCommand() => new StartBackfillValuationsSagaCommand(CorrelationId, PipelineMode, InstrumentId, StartDate, EndDate);
}