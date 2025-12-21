using Hoard.Core.Application;
using Hoard.Core.Application.Quotes;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("operations/quotes/")]
[Produces("application/json")]
[Tags("Operations")]
public class QuotesOperationsController(IMediator mediator) : ControllerBase
{
    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> RefreshQuotesAsync()
    {
        await mediator.SendAsync(new TriggerRefreshQuotesCommand());
        
        return Accepted(new { message = "Quote refresh triggered." });
    }
}