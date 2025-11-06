using Hoard.Core.Messages.Quotes;
using Microsoft.AspNetCore.Mvc;
using Rebus.Bus;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class QuotesController : ControllerBase
{
    private readonly IBus _bus;
    private readonly ILogger<QuotesController> _logger;

    public QuotesController(IBus bus, ILogger<QuotesController> logger)
    {
        _bus = bus;
        _logger = logger;
    }
    
    /// <summary>
    /// Triggers a batch refresh of all quotes.
    /// </summary>
    /// <remarks>
    /// Sends a <see cref="RefreshQuotesCommand"/> to the message bus.
    /// </remarks>
    /// <response code="202">Batch job accepted.</response>
    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> RefreshQuotesAsync()
    {
        _logger.LogInformation("Received request to refresh quotes.");

        await _bus.Send(new RefreshQuotesCommand(Guid.NewGuid()));
        
        return Accepted(new { message = "Quote refresh triggered." });
    }
}