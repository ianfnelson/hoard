using Hoard.Core.Application;
using Hoard.Core.Application.TransactionTypes;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("api/reference/transaction-types/")]
[Tags("Reference")]
[Produces("application/json")]
public class TransactionTypesController(IMediator mediator)
{
    [HttpGet]
    [ProducesResponseType(typeof(List<TransactionTypeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TransactionTypeDto>>> GetList(CancellationToken ct)
    {
        var query = new GetTransactionTypesQuery();
        
        var dtos = await mediator.QueryAsync<GetTransactionTypesQuery, List<TransactionTypeDto>>(query, ct);
        
        return new OkObjectResult(dtos);
    }
}