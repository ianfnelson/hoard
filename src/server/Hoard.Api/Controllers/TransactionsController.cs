using Hoard.Core.Application;
using Hoard.Core.Application.Transactions;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("transactions/")]
[Produces("application/json")]
[Tags("Transactions")]
public class TransactionsController(IMediator mediator) : ControllerBase
{
    [HttpGet("{id:int}")]
    public Task<ActionResult<TransactionDetailDto>> Get(int id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
    
    // TODO - add paginated endpoint for returning many transactions
    
    [HttpPost]
    public async Task<ActionResult<int>> Create([FromBody] TransactionWriteDto request, CancellationToken ct)
    {
        var id = await mediator.SendAsync<CreateTransactionCommand, int>(new CreateTransactionCommand(request), ct);
        return CreatedAtAction(nameof(Get), new { id }, id);
    }
    
    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(int id, [FromBody] TransactionWriteDto request, CancellationToken ct)
    {
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