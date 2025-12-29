using Hoard.Core.Application;
using Hoard.Core.Application.Instruments;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("api/instruments/")]
[Tags("Instruments")]
[Produces("application/json")]
public class InstrumentsController(IMediator mediator)
{
    [HttpGet("{id:int}")]
    public async Task<ActionResult<InstrumentDetailDto>> Get(int id, CancellationToken ct)
    {
        // TODO
        throw new NotImplementedException();
    }
    
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<InstrumentSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<InstrumentSummaryDto>>> GetList([FromQuery] GetInstrumentsQuery query, CancellationToken ct)
    {
        var dtos = await mediator.QueryAsync<GetInstrumentsQuery, PagedResult<InstrumentSummaryDto>>(query, ct);
        return new OkObjectResult(dtos);
    }
    
    [HttpPost]
    public async Task<ActionResult<int>> Create([FromBody] InstrumentWriteDto request, CancellationToken ct)
    {
        // TODO
        throw new NotImplementedException();
    }
    
    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(int id, [FromBody] InstrumentWriteDto request, CancellationToken ct)
    {
        // TODO
        throw new NotImplementedException();
    }
    
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        // TODO
        throw new NotImplementedException();
    }
    
    // TODO - prices endpoints - series and paginated
    // TODO - quote endpoint
}