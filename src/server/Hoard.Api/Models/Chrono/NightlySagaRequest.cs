using Hoard.Core.Application.Chrono;

namespace Hoard.Api.Models.Chrono;

public class NightlySagaRequest
{
    public DateOnly? AsOfDate { get; set; }

    public TriggerNightlyPreMidnightRunCommand ToPreMidnightCommand()
    {
        return new TriggerNightlyPreMidnightRunCommand(Guid.NewGuid(), AsOfDate);
    }
    
    public TriggerNightlyPostMidnightRunCommand ToPostMidnightCommand()
    {
        return new TriggerNightlyPostMidnightRunCommand(Guid.NewGuid(), AsOfDate);
    }
}