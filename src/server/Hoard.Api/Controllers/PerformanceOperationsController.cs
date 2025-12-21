using Hoard.Api.Models.Performance;
using Hoard.Core.Application;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("operations/performance/")]
[Tags("Operations")]
[Produces("application/json")]
public class PerformanceOperationsController(IMediator mediator, ILogger<PerformanceOperationsController> logger) : ControllerBase
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