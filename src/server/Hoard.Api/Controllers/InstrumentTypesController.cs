using Hoard.Core.Application;
using Hoard.Core.Application.InstrumentTypes;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("api/reference/instrument-types/")]
[Tags("Reference")]
[Produces("application/json")]
public class InstrumentTypesController(IMediator mediator)
{
    [HttpGet]
    [ProducesResponseType(typeof(List<InstrumentTypeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<InstrumentTypeDto>>> GetList(CancellationToken ct)
    {
        var query = new GetInstrumentTypesQuery();
        
        var dtos = await mediator.QueryAsync<GetInstrumentTypesQuery, List<InstrumentTypeDto>>(query, ct);
        
        return new OkObjectResult(dtos);
    }
}