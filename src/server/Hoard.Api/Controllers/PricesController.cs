using FluentValidation;
using Hoard.Core.Application;
using Hoard.Core.Application.Prices;
using Hoard.Core.Application.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("api/prices/")]
[Produces("application/json")]
[Tags("Prices")]
public class PricesController(IMediator mediator, IValidator<PriceWriteDto> validator) : ControllerBase
{
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(PriceDetailDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PriceDetailDto>> Get(int id, CancellationToken ct)
    {
        var query = new GetPriceQuery(id);
        var dto = await mediator.QueryAsync<GetPriceQuery, PriceDetailDto?>(query, ct);
        return dto == null ? new NotFoundResult() : new OkObjectResult(dto);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<int>> Create([FromBody] PriceWriteDto request, CancellationToken ct)
    {
        var problems = await validator.ValidateAndGetProblemsAsync(request, cancellationToken: ct);
        if (problems != null)
        {
            return BadRequest(problems);
        }

        var id = await mediator.SendAsync<CreatePriceCommand, int>(new CreatePriceCommand(request), ct);
        return CreatedAtAction(nameof(Get), new { id }, id);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Update(int id, [FromBody] PriceWriteDto request, CancellationToken ct)
    {
        var problems = await validator.ValidateAndGetProblemsAsync(request, entityId: id, cancellationToken: ct);
        if (problems != null)
        {
            return BadRequest(problems);
        }

        await mediator.SendAsync(new UpdatePriceCommand(id, request), ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await mediator.SendAsync(new DeletePriceCommand(id), ct);
        return NoContent();
    }

    // TODO - endpoint for bucketed price series
}