using Hoard.Core.Application.Performance;

namespace Hoard.Api.Models.Performances;

public class BackfillPerformancesRequest
{
    public int? InstrumentId { get; init; }
    
    public TriggerBackfillPerformancesCommand ToCommand()
    {
        return new TriggerBackfillPerformancesCommand(Guid.NewGuid(), InstrumentId);
    }
}