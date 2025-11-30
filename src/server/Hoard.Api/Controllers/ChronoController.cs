using Hoard.Api.Models.Chrono;
using Hoard.Core.Application;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ChronoController(IMediator mediator, ILogger<ChronoController> logger) : ControllerBase
{
    [HttpPost("trigger-close-of-day")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> TriggerCloseOfDay([FromBody] CloseOfDaySagaRequest model)
    {
        logger.LogInformation("Received request to trigger close of day saga");
        
        await mediator.SendAsync(model.ToCommand());
        
        return Accepted(new { message = "Close of day saga triggered." });
    }
}