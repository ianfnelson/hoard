using Hoard.Core.Domain.Entities;
using Hoard.Messages;
using Hoard.Messages.Prices;

namespace Hoard.Core.Application.Prices;

public record TriggerBackfillPricesCommand(
    int? InstrumentId,
    DateOnly? StartDate,
    DateOnly? EndDate,
    PipelineMode PipelineMode = PipelineMode.Backfill) : ITriggerCommand
{
    public Guid RunId { get; } = Guid.NewGuid();
    public int RunTypeId => RunType.BackfillPrices;
    public object ToBusCommand() => new StartRefreshPricesSagaCommand(
        RunId, PipelineMode, InstrumentId, StartDate, EndDate);
}