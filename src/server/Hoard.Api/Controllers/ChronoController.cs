using Hoard.Core.Application;
using Hoard.Core.Application.Chrono;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("api/operations/chrono/")]
[Tags("Operations")]
[Produces("application/json")]
public class ChronoController(IMediator mediator) : ControllerBase
{
    [HttpPost("trigger-close-of-day")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> TriggerCloseOfDay([FromBody] TriggerCloseOfDayRunCommand model)
    {
        await mediator.SendAsync(model);
        
        return Accepted(new { message = "Close of day saga triggered." });
    }
}