using Hoard.Core.Domain.Entities;
using Hoard.Messages;
using Hoard.Messages.Valuations;

namespace Hoard.Core.Application.Valuations;

public sealed record TriggerBackfillValuationsCommand(int? InstrumentId, DateOnly? StartDate, DateOnly? EndDate, PipelineMode PipelineMode = PipelineMode.Backfill) 
    : ITriggerCommand
{
    public Guid RunId { get; } = Guid.NewGuid();
    public int RunTypeId => RunType.BackfillValuations;
    public object ToBusCommand() => new StartBackfillValuationsSagaCommand(RunId, PipelineMode, InstrumentId, StartDate, EndDate);
}