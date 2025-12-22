using Hoard.Core.Application;
using Hoard.Core.Application.Performance;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("operations/performance/")]
[Tags("Operations")]
[Produces("application/json")]
public class PerformanceOperationsController(IMediator mediator) : ControllerBase
{
    [HttpPost("calculate")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> CalculatePerformanceAsync([FromBody] TriggerCalculatePerformanceCommand model)
    {
        await mediator.SendAsync(model);

        return Accepted(new { message = "Calculate performance triggered." });
    }
}