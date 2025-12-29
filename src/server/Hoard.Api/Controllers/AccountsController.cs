using Hoard.Core.Application;
using Hoard.Core.Application.Accounts;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("api/accounts/")]
[Tags("Accounts")]
[Produces("application/json")]
public class AccountsController(IMediator mediator)
{
    [HttpGet]
    [ProducesResponseType(typeof(List<AccountSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AccountSummaryDto>>> GetList([FromQuery] GetAccountsQuery query, CancellationToken ct)
    {
        var dtos = await mediator.QueryAsync<GetAccountsQuery, List<AccountSummaryDto>>(query, ct);
        
        return new OkObjectResult(dtos);
    }
    
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(AccountDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountDetailDto>> Get(int id, CancellationToken ct)
    {
        var query = new GetAccountQuery(id);
        
        var dto = await mediator.QueryAsync<GetAccountQuery, AccountDetailDto?>(query, ct);

        if (dto == null)
        {
            return new NotFoundResult();
        }

        return new OkObjectResult(dto);
    }
}