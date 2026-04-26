using Hoard.Core;
using Hoard.Core.Application;
using Hoard.Core.Application.Analysis;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("api/operations/analysis/")]
[Produces("application/json")]
[Tags("Operations")]
public class AnalysisOperationsController(IMediator mediator, TimeProvider timeProvider) : ControllerBase
{
    [HttpPost("analyse-instruments")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> AnalyseInstrumentsAsync([FromBody] AnalyseInstrumentsRequest request)
    {
        var asOfDate = request.AsOfDate ?? DateOnlyHelper.TodayLocal(timeProvider);
        await mediator.SendAsync(new TriggerAnalyseInstrumentsCommand(asOfDate));

        return Accepted(new { message = "Instrument analysis triggered." });
    }
}

public record AnalyseInstrumentsRequest(DateOnly? AsOfDate = null);
