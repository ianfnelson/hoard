using Hoard.Core.Domain.Entities;
using Hoard.Messages;
using Hoard.Messages.Prices;

namespace Hoard.Core.Application.Prices;

public sealed record TriggerRefreshPricesCommand(
    int? InstrumentId,
    DateOnly? StartDate,
    DateOnly? EndDate,
    PipelineMode PipelineMode = PipelineMode.DaytimeReactive) : ITriggerCommand
{
    public Guid RunId { get; } = Guid.NewGuid();
    public int RunTypeId => RunType.RefreshPrices;
    
    public object ToBusCommand() => new StartRefreshPricesSagaCommand(
        RunId, PipelineMode, InstrumentId, StartDate, EndDate);
}