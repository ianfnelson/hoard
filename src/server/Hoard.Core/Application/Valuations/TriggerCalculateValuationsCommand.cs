using Hoard.Core.Domain.Entities;
using Hoard.Messages;
using Hoard.Messages.Valuations;

namespace Hoard.Core.Application.Valuations;

public sealed record TriggerCalculateValuationsCommand(DateOnly? AsOfDate, PipelineMode PipelineMode = PipelineMode.DaytimeReactive) : 
    ITriggerCommand
{
    public Guid RunId { get; } = Guid.NewGuid();
    public int RunTypeId => RunType.CalculateValuations;
    
    public object ToBusCommand() => new StartCalculateValuationsSagaCommand(RunId, PipelineMode, null, AsOfDate);
}
