using Hoard.Core.Application;
using Hoard.Core.Application.AccountTypes;
using Hoard.Core.Application.AccountTypes.Models;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("reference/account-types/")]
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
    
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(AccountTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountTypeDto>> Get(int id, CancellationToken ct)
    {
        var query = new GetAccountTypeQuery(id);
        
        var dto = await mediator.QueryAsync<GetAccountTypeQuery, AccountTypeDto?>(query, ct);

        if (dto == null)
        {
            return new NotFoundResult();
        }

        return new OkObjectResult(dto);
    }
}