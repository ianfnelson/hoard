using Hoard.Messages;
using Hoard.Messages.Valuations;

namespace Hoard.Core.Application.Valuations;

public sealed record TriggerCalculateValuationsCommand(DateOnly? AsOfDate, PipelineMode PipelineMode = PipelineMode.DaytimeReactive) : 
    ITriggerCommand
{
    public Guid ValuationsRunId { get; } = Guid.NewGuid();
    
    public object ToBusCommand() => new StartCalculateValuationsSagaCommand(ValuationsRunId, PipelineMode, null, AsOfDate);
}
