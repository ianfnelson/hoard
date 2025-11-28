using Hoard.Messages;
using Hoard.Messages.Prices;

namespace Hoard.Core.Application.Prices;

public record TriggerBackfillPricesCommand(
    Guid CorrelationId,
    int? InstrumentId,
    DateOnly? StartDate,
    DateOnly? EndDate,
    PipelineMode PipelineMode = PipelineMode.Backfill) : ITriggerCommand
{
    public object ToBusCommand() => new StartRefreshPricesSagaCommand(
        CorrelationId, PipelineMode, InstrumentId, StartDate, EndDate);
}