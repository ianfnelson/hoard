using Hoard.Api.Models.Performance;
using Hoard.Core.Application;
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
    public async Task<IActionResult> CalculatePerformanceAsync([FromBody] CalculatePerformanceRequest model)
    {
        await mediator.SendAsync(model.ToCommand());

        return Accepted(new { message = "Calculate performance triggered." });
    }
}