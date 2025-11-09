using Hoard.Api.Models.Holdings;
using Microsoft.AspNetCore.Mvc;
using Rebus.Bus;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class HoldingsController : ControllerBase
{
    private readonly IBus _bus;
    private readonly ILogger<ValuationsController> _logger;
    
    public HoldingsController(IBus bus, ILogger<ValuationsController> logger)
    {
        _bus = bus;
        _logger = logger;
    }
    
    [HttpPost("backfill")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> BackfillHoldingsAsync([FromBody] BackfillHoldingsRequest model)
    {
        _logger.LogInformation("Received request to backfill holdings.");
        
        await _bus.Send(model.ToCommand());
        
        return Accepted(new { message = "Backfill holdings triggered." });
    }
    
    [HttpPost("calculate")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> CalculateHoldingsAsync([FromBody] CalculateHoldingsRequest model)
    {
        _logger.LogInformation("Received request to calculate holdings.");
        
        await _bus.Send(model.ToCommand());
        
        return Accepted(new { message = "Calculate holdings triggered." });
    }
}