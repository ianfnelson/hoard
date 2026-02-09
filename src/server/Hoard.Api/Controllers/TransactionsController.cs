using FluentValidation;
using Hoard.Core.Application;
using Hoard.Core.Application.Transactions;
using Hoard.Core.Application.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("api/transactions/")]
[Produces("application/json")]
[Tags("Transactions")]
public class TransactionsController(IMediator mediator, IValidator<TransactionWriteDto> validator) : ControllerBase
{
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(TransactionDetailDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<TransactionDetailDto>> Get(int id, CancellationToken ct)
    {
        var query = new GetTransactionQuery(id);
        var dto = await mediator.QueryAsync<GetTransactionQuery, TransactionDetailDto?>(query, ct);
        return dto == null ? new NotFoundResult() : new OkObjectResult(dto);
    }
    
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TransactionSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<TransactionSummaryDto>>> GetList([FromQuery] GetTransactionsQuery query, CancellationToken ct)
    {
        var dtos = await mediator.QueryAsync<GetTransactionsQuery, PagedResult<TransactionSummaryDto>>(query, ct);
        return new OkObjectResult(dtos);
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<int>> Create([FromBody] TransactionWriteDto request, CancellationToken ct)
    {
        var problems = await validator.ValidateAndGetProblemsAsync(request, cancellationToken: ct);
        if (problems != null)
        {
            return BadRequest(problems);
        }

        var id = await mediator.SendAsync<CreateTransactionCommand, int>(new CreateTransactionCommand(request), ct);
        return CreatedAtAction(nameof(Get), new { id }, id);
    }
    
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Update(int id, [FromBody] TransactionWriteDto request, CancellationToken ct)
    {
        var problems = await validator.ValidateAndGetProblemsAsync(request, entityId: id, cancellationToken: ct);
        if (problems != null)
        {
            return BadRequest(problems);
        }

        await mediator.SendAsync(new UpdateTransactionCommand(id, request), ct);
        return NoContent();
    }
    
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var command = new DeleteTransactionCommand(id);
        await mediator.SendAsync(command, ct);
        return NoContent();  // 204
    }
}