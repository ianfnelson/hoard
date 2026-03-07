using Hoard.Core.Application;
using Hoard.Core.Application.RunLog;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("api/run-logs")]
[Produces("application/json")]
[Tags("Run Logs")]
public class RunLogsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResult<RunLogDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRunLogsAsync([FromQuery] GetRunLogsQuery query, CancellationToken ct)
    {
        var result = await mediator.QueryAsync<GetRunLogsQuery, PagedResult<RunLogDto>>(query, ct);
        return Ok(result);
    }
}
