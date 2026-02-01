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
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(InstrumentDetailDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<InstrumentDetailDto>> Get(int id, CancellationToken ct)
    {
        var query = new GetInstrumentQuery(id);
        var dto = await mediator.QueryAsync<GetInstrumentQuery, InstrumentDetailDto?>(query, ct);
        return dto == null ? new NotFoundResult() : new OkObjectResult(dto);
    }
    
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<InstrumentSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<InstrumentSummaryDto>>> GetList([FromQuery] GetInstrumentsQuery query, CancellationToken ct)
    {
        var dtos = await mediator.QueryAsync<GetInstrumentsQuery, PagedResult<InstrumentSummaryDto>>(query, ct);
        return new OkObjectResult(dtos);
    }
    
    [HttpGet]                                                        
    [Route("lookup")]                                                
    [ProducesResponseType(typeof(List<LookupDto>), StatusCodes.Status200OK)]                                        
    public async Task<ActionResult<List<LookupDto>>> GetLookup(CancellationToken ct)                                  
    {                                                                
        var query = new GetInstrumentsLookupQuery();                 
        var dtos = await mediator.QueryAsync<GetInstrumentsLookupQuery, List<LookupDto>>(query, ct);                                     
        return new OkObjectResult(dtos);                             
    }  
    
    [HttpGet]
    [Route("{id:int}/prices")]
    [ProducesResponseType(typeof(PagedResult<PriceSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResult<PriceSummaryDto>>> GetPrices(int id, [FromQuery] GetPricesQuery query,
        CancellationToken ct)
    {
        query.InstrumentId = id;
        var dtos = await mediator.QueryAsync<GetPricesQuery, PagedResult<PriceSummaryDto>?>(query, ct);
        return dtos == null ? new NotFoundResult() : new OkObjectResult(dtos);
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
    
    // TODO - quote endpoint
}