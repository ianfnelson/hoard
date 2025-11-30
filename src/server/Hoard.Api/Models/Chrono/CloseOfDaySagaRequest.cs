using Hoard.Core.Application.Chrono;

namespace Hoard.Api.Models.Chrono;

public class CloseOfDaySagaRequest
{
    public DateOnly? AsOfDate { get; set; }
    
    public TriggerCloseOfDayRunCommand ToCommand()
    {
        return new TriggerCloseOfDayRunCommand(Guid.NewGuid(), AsOfDate);
    }
}