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
    
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(InstrumentTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InstrumentTypeDto>> Get(int id, CancellationToken ct)
    {
        var query = new GetInstrumentTypeQuery(id);
        
        var dto = await mediator.QueryAsync<GetInstrumentTypeQuery, InstrumentTypeDto?>(query, ct);

        if (dto == null)
        {
            return new NotFoundResult();
        }

        return new OkObjectResult(dto);
    }
}