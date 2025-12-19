using Hoard.Api.Models.Snapshots;
using Hoard.Core.Application;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class SnapshotsController(IMediator mediator, ILogger<SnapshotsController> logger) : ControllerBase
{
    [HttpPost("backfill")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> BackfillSnapshotsAsync([FromBody] BackfillSnapshotsRequest model)
    {
        logger.LogInformation("Received request to backfill snapshots.");
        
        await mediator.SendAsync(model.ToCommand());
        
        return Accepted(new { message = "Backfill snapshots triggered." });
    }

    [HttpPost("calculate")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> CalculateSnapshotsAsync([FromBody] CalculateSnapshotsRequest model)
    {
        logger.LogInformation("Received request to calculate snapshots.");

        await mediator.SendAsync(model.ToCommand());
        
        return Accepted(new { message = "Calculate snapshots triggered." });
    }
}