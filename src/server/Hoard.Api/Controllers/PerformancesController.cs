using Hoard.Api.Models.Performances;
using Hoard.Core.Application;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PerformancesController(IMediator mediator, ILogger<PerformancesController> logger) : ControllerBase
{
    [HttpPost("backfill")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> BackfillPerformancesAsync([FromBody] BackfillPerformancesRequest model)
    {
        logger.LogInformation("Received request to backfill performances.");

        await mediator.SendAsync(model.ToCommand());

        return Accepted(new { message = "Backfill performances triggered." });
    }
}