using Hoard.Api.Models.Valuations;
using Hoard.Core.Messages.Valuations;
using Microsoft.AspNetCore.Mvc;
using Rebus.Bus;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ValuationsController : ControllerBase
{
    private readonly IBus _bus;
    private readonly ILogger<ValuationsController> _logger;

    public ValuationsController(IBus bus, ILogger<ValuationsController> logger)
    {
        _bus = bus;
        _logger = logger;
    }
    
    /// <summary>
    /// Triggers calculation of all valuations
    /// </summary>
    /// <remarks>
    /// Sends a <see cref="CalculateValuationsCommand"/> to the message bus.
    /// </remarks>
    /// <response code="202">Batch job accepted.</response>
    [HttpPost("calculate")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> CalculateValuationsAsync([FromBody] CalculateValuationsRequest model)
    {
        _logger.LogInformation("Received request to calculate valuations.");

        await _bus.Send(model.ToCommand());
        
        return Accepted(new { message = "Calculate valuations triggered." });
    }
}