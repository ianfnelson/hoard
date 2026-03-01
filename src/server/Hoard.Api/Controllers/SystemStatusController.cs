using Hoard.Core.Application;
using Hoard.Core.Application.RunLog;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("api/system")]
[Produces("application/json")]
[Tags("System")]
public class SystemStatusController(IMediator mediator) : ControllerBase
{
    [HttpGet("run-logs")]
    [ProducesResponseType<PagedResult<RunLogDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRunLogsAsync([FromQuery] GetRunLogsQuery query, CancellationToken ct)
    {
        var result = await mediator.QueryAsync<GetRunLogsQuery, PagedResult<RunLogDto>>(query, ct);
        return Ok(result);
    }

    [HttpGet("run-types")]
    [ProducesResponseType<IReadOnlyList<RunTypeDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRunTypesAsync(CancellationToken ct)
    {
        var result = await mediator.QueryAsync<GetRunTypesQuery, IReadOnlyList<RunTypeDto>>(
            new GetRunTypesQuery(), ct);
        return Ok(result);
    }
}
