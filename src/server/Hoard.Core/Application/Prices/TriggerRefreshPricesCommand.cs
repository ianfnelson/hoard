using Hoard.Messages;
using Hoard.Messages.Prices;

namespace Hoard.Core.Application.Prices;

public record TriggerRefreshPricesCommand(
    Guid CorrelationId,
    int? InstrumentId,
    DateOnly? AsOfDate,
    PipelineMode PipelineMode = PipelineMode.DaytimeReactive) : ITriggerCommand
{
    public object ToBusCommand() => new StartRefreshPricesSagaCommand(
        CorrelationId, PipelineMode, InstrumentId, AsOfDate, AsOfDate);
}