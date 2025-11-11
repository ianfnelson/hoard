using Hoard.Core.Application;
using Hoard.Core.Application.Transactions;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TransactionsController(IMediator mediator, ILogger<TransactionsController> logger) : ControllerBase
{
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteTransaction(int id, CancellationToken ct)
    {
        logger.LogInformation("Received request to delete transaction {id}.", id);
        
        var command = new DeleteTransactionCommand(id);
        await mediator.SendAsync(command, ct);
        return NoContent();  // 204
    }
}