using Hoard.Core.Application;
using Hoard.Core.Application.Prices;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("api/operations/prices/")]
[Produces("application/json")]
[Tags("Operations")]
public class PricesOperationsController(IMediator mediator) : ControllerBase
{
    [HttpPost("backfill")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> BackfillPricesAsync([FromBody] TriggerBackfillPricesCommand model)
    {
        await mediator.SendAsync(model);
        
        return Accepted(new { message = "Prices backfill triggered." });
    }

    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> RefreshPricesAsync([FromBody] TriggerRefreshPricesCommand model)
    {
        await mediator.SendAsync(model);
        
        return Accepted(new { message = "Prices refresh triggered." });
    }
}