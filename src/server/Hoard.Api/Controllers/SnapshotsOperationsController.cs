using Hoard.Core.Application;
using Hoard.Core.Application.Snapshots;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("operations/snapshots/")]
[Produces("application/json")]
[Tags("Operations")]
public class SnapshotsOperationsController(IMediator mediator) : ControllerBase
{
    [HttpPost("backfill")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> BackfillSnapshotsAsync([FromBody] TriggerBackfillSnapshotsCommand model)
    {
        await mediator.SendAsync(model);
        
        return Accepted(new { message = "Backfill snapshots triggered." });
    }

    [HttpPost("calculate")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> CalculateSnapshotsAsync([FromBody] TriggerCalculateSnapshotsCommand model)
    {
        await mediator.SendAsync(model);
        
        return Accepted(new { message = "Calculate snapshots triggered." });
    }
}