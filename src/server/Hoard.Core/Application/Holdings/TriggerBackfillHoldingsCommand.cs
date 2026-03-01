using Hoard.Core.Domain.Entities;
using Hoard.Messages;
using Hoard.Messages.Holdings;

namespace Hoard.Core.Application.Holdings;

public sealed record TriggerBackfillHoldingsCommand(DateOnly? StartDate, DateOnly? EndDate, PipelineMode PipelineMode = PipelineMode.Backfill)
    : ITriggerCommand
{
    public Guid RunId { get; } = Guid.NewGuid();
    public int RunTypeId => RunType.BackfillHoldings;
    public object ToBusCommand() => new StartBackfillHoldingsSagaCommand(RunId, PipelineMode, StartDate, EndDate);
}