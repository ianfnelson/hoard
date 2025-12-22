using Hoard.Messages;
using Hoard.Messages.Holdings;

namespace Hoard.Core.Application.Holdings;

public sealed record TriggerCalculateHoldingsCommand(DateOnly? AsOfDate, PipelineMode PipelineMode = PipelineMode.DaytimeReactive) : 
    ITriggerCommand
{
    public Guid HoldingsRunId { get; } = Guid.NewGuid();
    
    public object ToBusCommand() => new CalculateHoldingsBusCommand(HoldingsRunId, PipelineMode, AsOfDate);
}