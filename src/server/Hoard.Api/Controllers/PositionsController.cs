using Hoard.Core.Application;
using Hoard.Core.Application.Positions;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("api/positions/")]
[Tags("Positions")]
[Produces("application/json")]
public class PositionsController(IMediator mediator)
{
    [HttpGet("{id:int}")]
    public async Task<ActionResult<PositionDetailDto>> Get(int id, CancellationToken ct)
    {
        // TODO
        throw new NotImplementedException();
    }
    
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<PositionSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<PositionSummaryDto>>> GetList([FromQuery] GetPositionsQuery query, CancellationToken ct)
    {
        // TODO - don't forget this response should include quote information
        // TODO
        throw new NotImplementedException();
    }

    // TODO - valuations, for charts and export
}