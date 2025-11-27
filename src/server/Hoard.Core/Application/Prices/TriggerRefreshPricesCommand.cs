using Hoard.Messages;
using Hoard.Messages.Prices;

namespace Hoard.Core.Application.Prices;

public record TriggerRefreshPricesCommand(
    Guid CorrelationId,
    PipelineMode PipelineMode,
    int? InstrumentId,
    DateOnly? StartDate,
    DateOnly? EndDate) : ITriggerCommand
{
    public object ToBusCommand() => new StartRefreshPricesSagaCommand(
        CorrelationId, PipelineMode, InstrumentId, StartDate, EndDate);
}