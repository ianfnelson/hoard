using Hoard.Core.Application;
using Hoard.Core.Application.Positions;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("operations/positions/")]
[Tags("Operations")]
[Produces("application/json")]
public class PositionsOperationsController(IMediator mediator, ILogger<PositionsOperationsController> logger) : ControllerBase
{
    [HttpPost("calculate")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> CalculatePositionsAsync()
    {
        logger.LogInformation("Received request to calculate positions.");

        await mediator.SendAsync(new TriggerCalculatePositionsCommand(Guid.NewGuid()));

        return Accepted(new { message = "Calculate positions triggered." });
    }
}