using Hoard.Core.Application;
using Hoard.Core.Application.AlertTypes;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("api/reference/alert-types/")]
[Tags("Reference")]
[Produces("application/json")]
public class AlertTypesController(IMediator mediator)
{
    [HttpGet]
    [ProducesResponseType(typeof(List<AlertTypeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AlertTypeDto>>> GetList(CancellationToken ct)
    {
        var query = new GetAlertTypesQuery();
        var dtos = await mediator.QueryAsync<GetAlertTypesQuery, List<AlertTypeDto>>(query, ct);
        return new OkObjectResult(dtos);
    }
}
