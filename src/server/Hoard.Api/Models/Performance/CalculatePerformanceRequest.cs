using Hoard.Core.Application.Performance;

namespace Hoard.Api.Models.Performance;

public class CalculatePerformanceRequest
{
    public int? InstrumentId { get; init; }
    
    public TriggerCalculatePerformanceCommand ToCommand()
    {
        return new TriggerCalculatePerformanceCommand(Guid.NewGuid(), InstrumentId);
    }
}