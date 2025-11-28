using Hoard.Core.Application;
using Hoard.Core.Application.Quotes;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class QuotesController(IMediator mediator, ILogger<QuotesController> logger) : ControllerBase
{
    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> RefreshQuotesAsync()
    {
        logger.LogInformation("Received request to refresh quotes.");

        await mediator.SendAsync(new TriggerRefreshQuotesCommand(Guid.NewGuid()));
        
        return Accepted(new { message = "Quote refresh triggered." });
    }
}