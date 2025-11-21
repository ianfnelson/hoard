using Hoard.Core.Application;
using Hoard.Core.Application.Positions;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PositionsController(IMediator mediator, ILogger<PositionsController> logger) : ControllerBase
{
    [HttpPost("rebuild")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> RebuildPositionsAsync()
    {
        logger.LogInformation("Received request to rebuild positions.");

        await mediator.SendAsync(new TriggerRebuildPositionsCommand(Guid.NewGuid()));

        return Accepted(new { message = "Rebuild positions triggered." });
    }
}