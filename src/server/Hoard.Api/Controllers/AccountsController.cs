using Hoard.Core.Application;
using Hoard.Core.Application.Accounts.Models;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("accounts/")]
[Tags("Accounts")]
[Produces("application/json")]
public class AccountsController(IMediator mediator)
{
    [HttpGet]
    public async Task<ActionResult<List<AccountSummaryDto>>> GetList([FromQuery] int portfolioId, CancellationToken ct)
    {
        // TODO - implement Accounts Get
        throw new NotImplementedException();
    }
    
    [HttpGet("{id:int}")]
    public async Task<ActionResult<AccountDetailDto>> Get(int id, CancellationToken ct)
    {
        // TODO - implement Accounts Get by ID
        throw new NotImplementedException();
    }
}