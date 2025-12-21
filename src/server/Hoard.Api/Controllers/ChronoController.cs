using Hoard.Api.Models.Chrono;
using Hoard.Core.Application;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("operations/chrono/")]
[Tags("Operations")]
[Produces("application/json")]
public class ChronoController(IMediator mediator) : ControllerBase
{
    [HttpPost("trigger-close-of-day")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> TriggerCloseOfDay([FromBody] CloseOfDaySagaRequest model)
    {
        await mediator.SendAsync(model.ToCommand());
        
        return Accepted(new { message = "Close of day saga triggered." });
    }
}