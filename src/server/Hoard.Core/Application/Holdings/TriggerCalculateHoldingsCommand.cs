using Hoard.Messages;
using Hoard.Messages.Holdings;

namespace Hoard.Core.Application.Holdings;

public record TriggerCalculateHoldingsCommand(Guid CorrelationId, PipelineMode PipelineMode, DateOnly? AsOfDate) : 
    ITriggerCommand
{
    public object ToBusCommand() => new CalculateHoldingsBusCommand(CorrelationId, PipelineMode, AsOfDate);
}