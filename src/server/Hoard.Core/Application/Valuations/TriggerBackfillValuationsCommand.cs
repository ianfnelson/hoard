using Hoard.Messages;
using Hoard.Messages.Valuations;

namespace Hoard.Core.Application.Valuations;

public sealed record TriggerBackfillValuationsCommand(int? InstrumentId, DateOnly? StartDate, DateOnly? EndDate, PipelineMode PipelineMode = PipelineMode.Backfill) 
    : ITriggerCommand
{
    public Guid ValuationsRunId { get; } = Guid.NewGuid();
    public object ToBusCommand() => new StartBackfillValuationsSagaCommand(ValuationsRunId, PipelineMode, InstrumentId, StartDate, EndDate);
}