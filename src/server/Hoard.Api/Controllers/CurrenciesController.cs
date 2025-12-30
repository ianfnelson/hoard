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
}