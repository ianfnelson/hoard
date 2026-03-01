using Hoard.Core.Domain.Entities;
using Hoard.Messages;
using Hoard.Messages.Holdings;

namespace Hoard.Core.Application.Holdings;

public sealed record TriggerCalculateHoldingsCommand(DateOnly? AsOfDate, PipelineMode PipelineMode = PipelineMode.DaytimeReactive) : 
    ITriggerCommand
{
    public Guid RunId { get; } = Guid.NewGuid();
    public int RunTypeId => RunType.CalculateHoldings;
    
    public object ToBusCommand() => new CalculateHoldingsBusCommand(RunId, PipelineMode, AsOfDate);
}