using Hoard.Core.Application;
using Hoard.Core.Application.AccountTypes;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("api/reference/account-types/")]
[Tags("Reference")]
[Produces("application/json")]
public class AccountTypesController(IMediator mediator)
{
    [HttpGet]
    [ProducesResponseType(typeof(List<AccountTypeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AccountTypeDto>>> GetList(CancellationToken ct)
    {
        var query = new GetAccountTypesQuery();
        
        var dtos = await mediator.QueryAsync<GetAccountTypesQuery, List<AccountTypeDto>>(query, ct);
        
        return new OkObjectResult(dtos);
    }
}