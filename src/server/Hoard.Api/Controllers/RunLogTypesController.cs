using Hoard.Core.Application;
using Hoard.Core.Application.RunLog;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("api/reference/run-log-types/")]
[Tags("Reference")]
[Produces("application/json")]
public class RunLogTypesController(IMediator mediator)
{
    [HttpGet]
    [ProducesResponseType(typeof(List<RunTypeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<RunTypeDto>>> GetList(CancellationToken ct)
    {
        var query = new GetRunTypesQuery();
        var dtos = await mediator.QueryAsync<GetRunTypesQuery, IReadOnlyList<RunTypeDto>>(query, ct);
        return new OkObjectResult(dtos);
    }
}
