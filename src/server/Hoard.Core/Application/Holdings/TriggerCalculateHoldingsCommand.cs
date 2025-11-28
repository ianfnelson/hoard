using Hoard.Messages;
using Hoard.Messages.Holdings;

namespace Hoard.Core.Application.Holdings;

public record TriggerCalculateHoldingsCommand(Guid CorrelationId, DateOnly? AsOfDate, PipelineMode PipelineMode = PipelineMode.DaytimeReactive) : 
    ITriggerCommand
{
    public object ToBusCommand() => new CalculateHoldingsBusCommand(CorrelationId, PipelineMode, AsOfDate);
}