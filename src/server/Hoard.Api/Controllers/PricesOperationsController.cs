using Hoard.Api.Models.Prices;
using Hoard.Core.Application;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("operations/prices/")]
[Produces("application/json")]
[Tags("Operations")]
public class PricesOperationsController(IMediator mediator) : ControllerBase
{
    [HttpPost("backfill")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> BackfillPricesAsync([FromBody] BackfillPricesRequest model)
    {
        await mediator.SendAsync(model.ToCommand());
        
        return Accepted(new { message = "Prices backfill triggered." });
    }

    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> RefreshPricesAsync([FromBody] RefreshPricesRequest model)
    {
        await mediator.SendAsync(model.ToCommand());
        
        return Accepted(new { message = "Prices refresh triggered." });
    }
}