using Hoard.Messages;
using Hoard.Messages.Valuations;

namespace Hoard.Core.Application.Valuations;

public record TriggerCalculateValuationsCommand(Guid CorrelationId, DateOnly? AsOfDate, PipelineMode PipelineMode = PipelineMode.DaytimeReactive) : 
    ITriggerCommand
{
    public object ToBusCommand() => new StartCalculateValuationsSagaCommand(CorrelationId, PipelineMode, null, AsOfDate);
}
