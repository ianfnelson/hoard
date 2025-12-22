using Hoard.Messages;
using Hoard.Messages.Prices;

namespace Hoard.Core.Application.Prices;

public sealed record TriggerRefreshPricesCommand(
    int? InstrumentId,
    DateOnly? StartDate,
    DateOnly? EndDate,
    PipelineMode PipelineMode = PipelineMode.DaytimeReactive) : ITriggerCommand
{
    public Guid PricesRunId { get; } = Guid.NewGuid();
    
    public object ToBusCommand() => new StartRefreshPricesSagaCommand(
        PricesRunId, PipelineMode, InstrumentId, StartDate, EndDate);
}