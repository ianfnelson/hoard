using FluentValidation;
using Hoard.Core.Application;
using Hoard.Core.Application.Instruments;
using Hoard.Core.Application.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("api/instruments/")]
[Tags("Instruments")]
[Produces("application/json")]
public class InstrumentsController(IMediator mediator, IValidator<InstrumentWriteDto> validator) : ControllerBase
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
    [Route("{id:int}/price-series")]
    [ProducesResponseType(typeof(PriceSeriesDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PriceSeriesDto>> GetPriceSeries(int id, DateOnly? from, DateOnly? to,
        CancellationToken ct)
    {
        var query = new GetPriceSeriesQuery(id) { From = from, To = to };
        var dto = await mediator.QueryAsync<GetPriceSeriesQuery, PriceSeriesDto?>(query, ct);
        return dto == null ? new NotFoundResult() : new OkObjectResult(dto);
    }

    [HttpGet]
    [Route("{id:int}/valuation-series")]
    [ProducesResponseType(typeof(ValuationSeriesDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ValuationSeriesDto>> GetValuationSeries(int id, DateOnly? from, DateOnly? to,
        CancellationToken ct)
    {
        var query = new GetValuationSeriesQuery(id) { From = from, To = to };
        var dto = await mediator.QueryAsync<GetValuationSeriesQuery, ValuationSeriesDto?>(query, ct);
        return dto == null ? new NotFoundResult() : new OkObjectResult(dto);
    }

    [HttpGet]
    [Route("{id:int}/prices")]
    [ProducesResponseType(typeof(PagedResult<PriceSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResult<PriceSummaryDto>>> GetPrices(int id, int pageNumber = 1,
        int pageSize = 10, CancellationToken ct = default)
    {
        var query = new GetPricesQuery(id) { PageNumber = pageNumber, PageSize = pageSize };
        var dtos = await mediator.QueryAsync<GetPricesQuery, PagedResult<PriceSummaryDto>?>(query, ct);
        return dtos == null ? new NotFoundResult() : new OkObjectResult(dtos);
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<int>> Create([FromBody] InstrumentWriteDto request, CancellationToken ct)
    {
        var problems = await validator.ValidateAndGetProblemsAsync(request, cancellationToken: ct);
        if (problems != null)
        {
            return BadRequest(problems);
        }

        var id = await mediator.SendAsync<CreateInstrumentCommand, int>(new CreateInstrumentCommand(request), ct);
        return CreatedAtAction(nameof(Get), new { id }, id);
    }
    
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Update(int id, [FromBody] InstrumentWriteDto request, CancellationToken ct)
    {
        var problems = await validator.ValidateAndGetProblemsAsync(request, entityId: id, cancellationToken: ct);
        if (problems != null)
        {
            return BadRequest(problems);
        }

        await mediator.SendAsync(new UpdateInstrumentCommand(id, request), ct);
        return NoContent();
    }
    
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await mediator.SendAsync(new DeleteInstrumentCommand(id), ct);
        return NoContent();  // 204
    }
}