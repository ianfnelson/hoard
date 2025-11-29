using Hoard.Api.Models.Chrono;
using Hoard.Api.Models.Holdings;
using Hoard.Core.Application;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ChronoController(IMediator mediator, ILogger<ChronoController> logger) : ControllerBase
{
    [HttpPost("trigger-nightly-pre-midnight")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> TriggerNightlyPreMidnight([FromBody] NightlySagaRequest model)
    {
        logger.LogInformation("Received request to trigger nightly pre-midnight saga");
        
        await mediator.SendAsync(model.ToPreMidnightCommand());
        
        return Accepted(new { message = "Nightly pre-midnight saga triggered." });
    }
    
    [HttpPost("trigger-nightly-post-midnight")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> TriggerNightlyPostMidnight([FromBody] NightlySagaRequest model)
    {
        logger.LogInformation("Received request to trigger nightly post-midnight saga");
        
        await mediator.SendAsync(model.ToPostMidnightCommand());
        
        return Accepted(new { message = "Nightly post-midnight saga triggered." });
    }
}