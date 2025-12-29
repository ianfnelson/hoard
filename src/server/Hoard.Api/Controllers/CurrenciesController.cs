using Hoard.Core.Application;
using Hoard.Core.Application.Currencies;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("api/reference/currencies/")]
[Tags("Reference")]
[Produces("application/json")]
public class CurrenciesController(IMediator mediator)
{
    [HttpGet]
    [ProducesResponseType(typeof(List<CurrencyDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CurrencyDto>>> GetList(CancellationToken ct)
    {
        var query = new GetCurrenciesQuery();
        
        var dtos = await mediator.QueryAsync<GetCurrenciesQuery, List<CurrencyDto>>(query, ct);
        
        return new OkObjectResult(dtos);
    }
    
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CurrencyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CurrencyDto>> Get(string id, CancellationToken ct)
    {
        var query = new GetCurrencyQuery(id);
        
        var dto = await mediator.QueryAsync<GetCurrencyQuery, CurrencyDto?>(query, ct);

        if (dto == null)
        {
            return new NotFoundResult();
        }

        return new OkObjectResult(dto);
    }
}