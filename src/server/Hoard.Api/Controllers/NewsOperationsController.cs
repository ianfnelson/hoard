using Hoard.Core.Application;
using Hoard.Core.Application.News;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("api/operations/news/")]
[Produces("application/json")]
[Tags("Operations")]
public class NewsOperationsController(IMediator mediator) : ControllerBase
{
    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> RefreshNewsAsync([FromBody] TriggerRefreshNewsCommand model)
    {
        await mediator.SendAsync(model);

        return Accepted(new { message = "News refresh triggered." });
    }
}
