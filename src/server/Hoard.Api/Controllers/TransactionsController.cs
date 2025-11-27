using Hoard.Core.Application;
using Hoard.Core.Application.Transactions;
using Hoard.Core.Application.Transactions.Models;
using Hoard.Messages;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TransactionsController(IMediator mediator, ILogger<TransactionsController> logger) : ControllerBase
{
    [HttpGet("{id:int}")]
    public Task<ActionResult<TransactionDto>> Get(int id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
    
    [HttpPost]
    public async Task<ActionResult<int>> Create([FromBody] TransactionWriteDto request, CancellationToken ct)
    {
        var id = await mediator.SendAsync<CreateTransactionCommand, int>(new CreateTransactionCommand(Guid.NewGuid(), PipelineMode.DaytimeReactive, request), ct);
        return CreatedAtAction(nameof(Get), new { id }, id);
    }
    
    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(int id, [FromBody] TransactionWriteDto request, CancellationToken ct)
    {
        await mediator.SendAsync(new UpdateTransactionCommand(Guid.NewGuid(), PipelineMode.DaytimeReactive, id, request), ct);
        return NoContent();
    }
    
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteTransaction(int id, CancellationToken ct)
    {
        logger.LogInformation("Received request to delete transaction {id}.", id);
        
        var command = new DeleteTransactionCommand(Guid.NewGuid(), PipelineMode.DaytimeReactive, id);
        await mediator.SendAsync(command, ct);
        return NoContent();  // 204
    }
}