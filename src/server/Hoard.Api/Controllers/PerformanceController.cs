using Hoard.Api.Models.Performance;
using Hoard.Core.Application;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PerformanceController(IMediator mediator, ILogger<PerformanceController> logger) : ControllerBase
{
    [HttpPost("calculate")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> CalculatePerformanceAsync([FromBody] CalculatePerformanceRequest model)
    {
        logger.LogInformation("Received request to calculate performance.");

        await mediator.SendAsync(model.ToCommand());

        return Accepted(new { message = "Calculate performance triggered." });
    }
}