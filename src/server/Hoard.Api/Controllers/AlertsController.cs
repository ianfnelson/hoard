using Hoard.Core.Application;
using Hoard.Core.Application.Alerts;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("api/alerts/")]
[Produces("application/json")]
[Tags("Alerts")]
public class AlertsController(IMediator mediator) : ControllerBase
{
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(AlertDetailDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AlertDetailDto>> Get(int id, CancellationToken ct)
    {
        var query = new GetAlertQuery(id);
        var dto = await mediator.QueryAsync<GetAlertQuery, AlertDetailDto?>(query, ct);
        return dto == null ? new NotFoundResult() : new OkObjectResult(dto);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AlertSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<AlertSummaryDto>>> GetList([FromQuery] GetAlertsQuery query, CancellationToken ct)
    {
        var dtos = await mediator.QueryAsync<GetAlertsQuery, PagedResult<AlertSummaryDto>>(query, ct);
        return new OkObjectResult(dtos);
    }
}
